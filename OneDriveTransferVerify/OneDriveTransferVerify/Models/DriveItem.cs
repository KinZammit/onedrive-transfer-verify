namespace OneDriveTransferVerify.Models
{
    internal class DriveItem
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required long Size { get; init; }
    }
}