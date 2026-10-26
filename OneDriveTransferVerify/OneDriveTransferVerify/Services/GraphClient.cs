using OneDriveTransferVerify.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Net.Mime.MediaTypeNames;

namespace OneDriveTransferVerify.Services
{
    internal class GraphClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public GraphClient(AuthService authService)
        {
            _authService = authService;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
        }

        //Not required for Task, using to verify sign-in
        public async Task<string> GetCurrentUserAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "me");
            var response = await SendAuthorizedRequestAsync(request);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> FolderExistsAsync(string folderName)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"me/drive/root:/{Uri.EscapeDataString(folderName)}");
            var response = await SendAuthorizedRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Folder check failed: {(int)response.StatusCode} {response.StatusCode}. {error}",
                    null,
                    response.StatusCode);
            }

            //Must be 2xx at this stage
            return true;
        }

        public async Task CreateFolderAsync(string folderName)
        {
            var body = new CreateFolderRequest { Name = folderName };

            using var request = new HttpRequestMessage(HttpMethod.Post, "me/drive/root/children");
            request.Content = JsonContent.Create(body);

            var response = await SendAuthorizedRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return; //Since this means Folder exists

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Create folder failed: {(int)response.StatusCode} {response.StatusCode}. {error}",
                    null,
                    response.StatusCode);
            }

        }

        public async Task<(DriveItem Item, bool Created)> UploadAsync(string folderName, string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            await using var fileStream = File.OpenRead(filePath);

            using var request = new HttpRequestMessage(HttpMethod.Put, $"me/drive/root:/{Uri.EscapeDataString(folderName)}/{Uri.EscapeDataString(fileName)}:/content");

            request.Content = new StreamContent(fileStream); //as per docs: file is the body
            //This was researched since assignment specified: e.g., a sample text or binary file)
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await SendAuthorizedRequestAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"File failed to upload: {(int)response.StatusCode} {response.StatusCode}. {error}",
                    null,
                    response.StatusCode);
            }

            var item = await response.Content.ReadFromJsonAsync<DriveItem>()
                ?? throw new HttpRequestException("Upload should return Drive Item");

            var created = response.StatusCode == HttpStatusCode.Created;

            return (item, created);
        }

        public async Task DownloadAsync(string itemId, string destinationPath)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"me/drive/items/{Uri.EscapeDataString(itemId)}/content");
            Console.WriteLine(request.RequestUri);

            var response = await SendAuthorizedRequestAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"File failed to download: {(int)response.StatusCode} {response.StatusCode}. {error}",
                    null,
                    response.StatusCode);
            }

            await using var downloadStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(destinationPath);
            await downloadStream.CopyToAsync(fileStream); //Copies the file in batches (streams)
        }

        //All Requests are sent through this helper to avoid repetition
        private async Task<HttpResponseMessage> SendAuthorizedRequestAsync(HttpRequestMessage request)
        {
            var token = await _authService.GetAccessTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }
    }
}
