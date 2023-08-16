using Newtonsoft.Json;

namespace Technologai.Agents.Models
{
    public class DallERequestData
    {
        [JsonProperty("prompt")]
        public string InputText { get; set; } = null!;
        [JsonProperty("n")]
        public int NoOfImages { get; set; }
        [JsonProperty("size")]
        public string ImageSize { get; set; } = null!;
    }
}
