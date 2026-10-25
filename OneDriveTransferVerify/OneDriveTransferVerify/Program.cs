using OneDriveTransferVerify;
using OneDriveTransferVerify.Services;
using System.Text.Json;

var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
var json = File.ReadAllText(settingsPath);
var settings = JsonSerializer.Deserialize<AppSettings>(json);

var authService = new AuthService(settings.ClientId);
var token = await authService.GetAccessTokenAsync();

var client = new GraphClient(authService);

//For Testing - not required
var me = await client.GetCurrentUserAsync();
Console.WriteLine(me);

var exists = await client.FolderExistsAsync(settings.FolderName);
if(!exists)
{
    await client.CreateFolderAsync(settings.FolderName);
    Console.WriteLine("Folder Created");
}

else
{
    Console.WriteLine("Folder already exists");
}

