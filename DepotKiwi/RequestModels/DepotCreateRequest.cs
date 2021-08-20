using System.Text.Json.Serialization;

namespace DepotKiwi.RequestModels {
    public class DepotCreateRequest {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}