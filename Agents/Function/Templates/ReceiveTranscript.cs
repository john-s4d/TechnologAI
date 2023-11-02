using Newtonsoft.Json;
using System.Text;
using Technologai.Agents.Core.DataModels;

namespace Technologai.Templates
{
    /*
    //Receive Transcript
    ReceiveTranscript receiveTranscript = new();
    var receiveTranscript_Dict = new Dictionary<string, object>()
        {
            {"apiKey","4a707596c9a93478b4d8f3ada9b3eaa742fa0035"},
            {"audioUrl",$"https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"}
        };
    // var receiveTranscriptResponse =await receiveTranscript.Execute(receiveTranscript_Dict);
    */

    /// <summary>
    /// Receive Transcript
    /// </summary>
    public class ReceiveTranscript : Template
    {
        internal string ApiKey { get; set; } = string.Empty;
        public ReceiveTranscript(string apiKey)
        {
            Id = "receive_transcript";
            Description = "Receive transcript using deepgram api key";
            InputKeys = new string[] { "audioUrl" };
            OutputKeys = new string[] { "contents" }; 
            ApiKey = apiKey;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            var audioUrl = ((string)information.Input.Structured["audioUrl"]);
            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("Authorization", "Token " + ApiKey);
                RequestReceiveTranscriptModel reqData = new()
                {
                    url = audioUrl
                };
                var jsonReq = JsonConvert.SerializeObject(reqData);
                var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.deepgram.com/v1/listen?model=nova&punctuate=true", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var contents = JsonConvert.SerializeObject(responseContent);
                    return Data.Create(contents);
                }
                else
                {
                    return Data.Create("Error", $"Unable to generate transcript :'{response.StatusCode} - {response.ReasonPhrase}'");
                }
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
        }
    }
}