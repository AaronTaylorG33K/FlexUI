using System.Text.Json.Serialization;

namespace FlexUI.Models
{
    public class Mutation
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty; // Initialize with default value

        [JsonPropertyName("newOrdinal")]
        public int NewOrdinal { get; set; }

        [JsonPropertyName("destinationPageID")]
        public int DestinationPageID { get; set; }

        [JsonPropertyName("pageComponentID")]
        public int PageComponentID { get; set; }

        [JsonPropertyName("componentName")]
        public string ComponentName { get; set; } = string.Empty; // Initialize with default value
    }
}