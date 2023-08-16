using Newtonsoft.Json;

public class EmbeddingsRequestDataModel
{
    [JsonProperty("input")]
    public string InputText { get; set; } = null!;

    [JsonProperty("model")]
    public string EmbeddingsModel { get; set; } = null!;
}

