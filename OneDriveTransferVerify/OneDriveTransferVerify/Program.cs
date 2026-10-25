using Microsoft.Identity.Client;
using OneDriveTransferVerify;
using OneDriveTransferVerify.Services;
using System.Net.Http.Headers;
using System.Text.Json;

var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
var json = File.ReadAllText(settingsPath);
var settings = JsonSerializer.Deserialize<AppSettings>(json);

var authService = new AuthService(settings.ClientId);
var token = await authService.GetAccessTokenAsync();

var client = new HttpClient();
client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

var response = await client.GetAsync("me");

var body = await response.Content.ReadAsStringAsync();
Console.WriteLine($"{(int)response.StatusCode} {response.StatusCode}");
Console.WriteLine(body);