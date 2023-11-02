using Newtonsoft.Json;
using System.Text;

namespace Technologai.Templates
{
    /// <summary>
    /// TranscriptAudioVoiceFile
    public class TrascriptAudioVoiceFile : Template
    {

        /*
        // Trascript Audio Voice File 
        TrascriptAudioVoiceFile trascriptAudioVoiceFile = new();
        var transcriptAudioVoiceFileDict = new Dictionary<string, object>()
                {
                    {"API_Key","assemblyAI api key"},
                    {"filepath",@"directory of local system with extension" }
                };
        var responseFromTranscriptAudioFile = trascriptAudioVoiceFile.Execute(transcriptAudioVoiceFileDict).Result;
        var deserialisedData = JsonConvert.DeserializeObject<TranscriptModel>(responseFromTranscriptAudioFile["contents"].ToString());
                if (deserialisedData.text == null)
                {
                    Console.WriteLine("Copy to Trnsacript Id to see the result of trascript");
                    Console.WriteLine("Trnsacript Id :" + deserialisedData.id);
                }
                else
        {
            Console.WriteLine("Listed below your transcript");
            Console.WriteLine(deserialisedData.text);
        }
            */

        public string Description { get; } = "Trascript Audio Voice Stream";
        public string SampleJsonIn { get; set; } = "{\"ApiKey\":\"string\",\"FilePath\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            Console.WriteLine("Please select 1. to send the file.");
            Console.WriteLine("Please select 2. to check the file if it's ready.");

            // saving path in local system
            string filePath = ((string)data["filepath"]).Trim();
            //reading api key
            var api_Key = ((string)data["API_Key"]).Trim();

            var operation = Console.ReadLine();

            if (operation == "1")
            {
                HttpClient httpClient = new()
                {
                    BaseAddress = new Uri("https://api.assemblyai.com/v2/")
                };
                httpClient.DefaultRequestHeaders.Add("authorization", api_Key);

                string jsonResult = SendFile(httpClient, filePath).Result;
                Console.WriteLine(jsonResult);

                //cleaning the old HttpClient connection
                httpClient.Dispose();
                httpClient = new();
                httpClient.BaseAddress = new Uri("https://api.assemblyai.com/v2/");
                //add the request header which is our api key paste you api key here
                httpClient.DefaultRequestHeaders.Add("authorization", api_Key);

                var json = new { audio_url = JsonConvert.DeserializeObject<string>(jsonResult) };
                //create a string content from our JSON which we will need for our next request
                StringContent payload = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");

                HttpResponseMessage responseMessage = await httpClient.PostAsync("https://api.assemblyai.com/v2/transcript", payload);
                //throw an exception if the request did not go through
                responseMessage.EnsureSuccessStatusCode();
                //display the data
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                return new Dictionary<string, object> { { "contents", responseJson } };

                Console.WriteLine(responseJson);
            }
            else if (operation == "2")
            {
                Console.WriteLine("Please enter the ID");
                string ticketID = Console.ReadLine();

                using (HttpClient httpClient = new HttpClient())
                {
                    //pass the API key
                    httpClient.DefaultRequestHeaders.Add("authorization", api_Key);
                    //set the header to json
                    httpClient.DefaultRequestHeaders.Add("Accepts", "application/json");
                    //send a get request to the transcript enpoint and add the ticketId to the url
                    HttpResponseMessage responseMessage = await httpClient.GetAsync("https://api.assemblyai.com/v2/transcript/" + ticketID);
                    //make sure the call wet through
                    responseMessage.EnsureSuccessStatusCode();
                    //dispaly the data
                    var responseJson = await responseMessage?.Content.ReadAsStringAsync();
                    return new Dictionary<string, object> { { "contents", responseJson } };
                }
            }
            else
            {
                Console.WriteLine("Please select 1 or 2 to perform operation");
                return new Dictionary<string, object> { { "content", "Please enter valid Keyword like 1, 2 " } };
            }
        }
        static async Task<string> SendFile(HttpClient httpClient, string filePath)
        {
            try
            {
                HttpRequestMessage requestMessage = new(HttpMethod.Post, "upload");
                requestMessage.Headers.Add("Transer-Encoding", "chunked");
                var fileReader = System.IO.File.OpenRead(filePath);
                var streamContent = new StreamContent(fileReader);
                requestMessage.Content = streamContent;
                //send the request
                HttpResponseMessage httpResponse = await httpClient.SendAsync(requestMessage);
                //return the reponse as a string
                return await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception:  {ex.Message}");
                throw;
            }
        }
    }
}