using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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

        //Not asked for Task, using it as testing
        public async Task<string>GetCurrentUserAsync()
        {
            var token = await _authService.GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, "me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
