using System.Text.Json.Serialization;

namespace OneDriveTransferVerify.Models
{
    internal class CreateFolderRequest
    {
        public required string Name { get; init; }
        public object Folder { get; init; } = new();

        [JsonPropertyName("@microsoft.graph.conflictBehavior")]
        public string ConflictBehavior { get; init; } = "fail";
    }
}