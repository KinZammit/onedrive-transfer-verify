using Microsoft.Identity.Client;
using System.Net.Http.Headers;

string[] scopes = new string[] { "User.read", "Files.ReadWrite" };

var app = PublicClientApplicationBuilder.Create("7667b210-b4b4-4d50-9c9c-5f3928569314")
    .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
    .WithRedirectUri("http://localhost")
    .Build();

var accounts = await app.GetAccountsAsync();

AuthenticationResult result;

try
{
    result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
      .ExecuteAsync();
}
catch (MsalUiRequiredException)
{
    result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
}

var client = new HttpClient();
client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

var response = await client.GetAsync("me");

var body = await response.Content.ReadAsStringAsync();
Console.WriteLine($"{(int)response.StatusCode} {response.StatusCode}");
Console.WriteLine(body);