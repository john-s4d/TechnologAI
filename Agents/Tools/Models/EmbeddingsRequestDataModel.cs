using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai_Second_Phase.Models.GetEmbeddingsModel
{
    public class RequestDataModel
    {
        [JsonProperty("input")]
        public string InputText { get; set; } = null!;
        [JsonProperty("model")]
        public string EmbeddingsModel { get; set; } = null!;
    }
}
