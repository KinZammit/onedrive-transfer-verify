using System.Security.Cryptography;

namespace OneDriveTransferVerify.Verification
{
    internal static class FileHasher
    {
        public static async Task<string> ComputeSha256Async(string filePath)
        {
            await using var stream = File.OpenRead(filePath);

            var hashBytes = await SHA256.HashDataAsync(stream);

            return Convert.ToHexString(hashBytes);
        }
    }
}
