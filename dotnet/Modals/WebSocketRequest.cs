using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlexUI.Models
{
    public class WebSocketRequest
    {
        [JsonPropertyName("mutations")]
        public List<Mutation> Mutations { get; set; } = new List<Mutation>(); // Initialize with default value
    }
}