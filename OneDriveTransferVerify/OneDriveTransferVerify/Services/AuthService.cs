using Microsoft.Identity.Client;

namespace OneDriveTransferVerify.Services
{
    internal class AuthService
    {
        private readonly IPublicClientApplication _app;
        private readonly string[] _scopes = { "User.Read", "Files.ReadWrite" };

        public AuthService(string clientId) 
        {
            _app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri("http://localhost")
                .Build();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var accounts = await _app.GetAccountsAsync();

            AuthenticationResult result;

            try
            {
                result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                  .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await _app.AcquireTokenInteractive(_scopes).ExecuteAsync();
            }

            return result.AccessToken;

        }
    }


}
