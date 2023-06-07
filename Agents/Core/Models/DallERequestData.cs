using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
