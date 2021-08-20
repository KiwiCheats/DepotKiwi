using System.Text.Json.Serialization;

namespace DepotKiwi.RequestModels {
    public class DepotDeleteRequest {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}