using Newtonsoft.Json;
using System.Text;
using Technologai.Agents.Core.DataModels;

namespace Technologai
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
        public new string Description { get; set; } = "Receive transcript using deepgram api key";
        public new string SampleJsonIn { get; set; } = "{\"apiKey\":\"string\",\"audioUrl\":\"string\"}";
        public new string SampleJsonOut { get; set; } = "{\"contents\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var apiKey = ((string)data["apiKey"]);
            var audioUrl = ((string)data["audioUrl"]);
            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("Authorization", "Token " + apiKey);
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
                    var jsonResponse = JsonConvert.SerializeObject(responseContent);
                    return new Dictionary<string, object> { { "contents", jsonResponse } };
                }
                else
                {
                    return new Dictionary<string, object> { { "Error", $"Unable to generate transcript :'{response.StatusCode} - {response.ReasonPhrase}'" } };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "Exception occured", $"'{ex.Message}'" } };
            }
        }
    }
}