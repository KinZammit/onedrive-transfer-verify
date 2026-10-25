using OneDriveTransferVerify.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        //All Requests are sent through this helper to avoid repetition
        private async Task<HttpResponseMessage> SendAuthorizedRequestAsync(HttpRequestMessage request)
        {
            var token = await _authService.GetAccessTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }
    }
}
