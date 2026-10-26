using OneDriveTransferVerify;
using OneDriveTransferVerify.Services;
using OneDriveTransferVerify.Verification;
using System.Diagnostics;
using System.Text.Json;

var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
var json = File.ReadAllText(settingsPath);
var settings = JsonSerializer.Deserialize<AppSettings>(json);

var filePath = Path.Combine(AppContext.BaseDirectory, "sample.txt");
var originalHash = await FileHasher.ComputeSha256Async(filePath); //hashed the orginal now  before upload so any change/update in the file would fail
Debug.Assert(originalHash.Length == 64, "Expected SHA to be 64 hex Characters");
Console.WriteLine($"{originalHash}");

var authService = new AuthService(settings.ClientId);
var graphClient = new GraphClient(authService);

//For Testing - not required
var me = await graphClient.GetCurrentUserAsync();
Console.WriteLine(me);

var exists = await graphClient.FolderExistsAsync(settings.UploadFolderName);
if(!exists)
{
    await graphClient.CreateFolderAsync(settings.UploadFolderName);
    Console.WriteLine("Folder Created");
}

else
{
    Console.WriteLine("Folder already exists");
}

var (uploaded, created) = await graphClient.UploadAsync(settings.UploadFolderName, filePath);
Console.WriteLine($"Uploaded {uploaded.Name} - {(created ? "File was created" : "File was replaced")}");

var localSize = new FileInfo(filePath).Length;

if (uploaded.Size !=  localSize)
{
    Console.WriteLine($"files differ in size");
    return 1;
}

var downloadDir = Path.Combine(Path.GetTempPath(), settings.DownloadFolderName);
Directory.CreateDirectory(downloadDir);
var downloadPath = Path.Combine(downloadDir, uploaded.Name);

//Added this to further solidify original and downloaded would not be the same
if (string.Equals(
        Path.GetFullPath(filePath),
        Path.GetFullPath(downloadPath),
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Download path is the same as the original file, not to overwrite.");
    return 1;
}

await graphClient.DownloadAsync(uploaded.Id, downloadPath);
Console.WriteLine($"Downloaded to: {downloadPath}");

return 0;
    