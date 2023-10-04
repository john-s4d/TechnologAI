using Newtonsoft.Json;
using System.Text;

namespace Technologai.Templates
{
    /// <summary>
    /// TranscriptAudioVoiceFile
    public class TrascriptAudioVoiceFile : Template
    {

        internal string ApiKey { get; set; } = string.Empty;
        public TrascriptAudioVoiceFile(string apiKey)
        {
            Id = "trascript_audio_voice_file";
            Description = "Trascript Audio Voice Stream";
            InputKeys = new string[] { "filepath" };
            OutputKeys = new string[] { "content" };  
            ApiKey = apiKey;    
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {

                Console.WriteLine("Please select 1. to send the file.");
                Console.WriteLine("Please select 2. to check the file if it's ready.");

                // saving path in local system
                string filePath = ((string)information.Input.Structured["filepath"]).Trim();
                //reading api key
                ApiKey = ApiKey.Trim();

                var operation = Console.ReadLine();

                if (operation == "1")
                {
                    HttpClient httpClient = new()
                    {
                        BaseAddress = new Uri("https://api.assemblyai.com/v2/")
                    };
                    httpClient.DefaultRequestHeaders.Add("authorization", ApiKey);

                    string jsonResult = SendFile(httpClient, filePath).Result;
                    Console.WriteLine(jsonResult);

                    //cleaning the old HttpClient connection
                    httpClient.Dispose();
                    httpClient = new();
                    httpClient.BaseAddress = new Uri("https://api.assemblyai.com/v2/");
                    //add the request header which is our api key paste you api key here
                    httpClient.DefaultRequestHeaders.Add("authorization", ApiKey);

                    var json = new { audio_url = JsonConvert.DeserializeObject<string>(jsonResult) };
                    //create a string content from our JSON which we will need for our next request
                    StringContent payload = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");

                    HttpResponseMessage responseMessage = await httpClient.PostAsync("https://api.assemblyai.com/v2/transcript", payload);
                    //throw an exception if the request did not go through
                    responseMessage.EnsureSuccessStatusCode();
                    //display the data
                    var content = await responseMessage.Content.ReadAsStringAsync();
                    return Data.Create(content);
                }
                else if (operation == "2")
                {
                    Console.WriteLine("Please enter the ID");
                    string ticketID = Console.ReadLine();

                    using (HttpClient httpClient = new HttpClient())
                    {
                        //pass the API key
                        httpClient.DefaultRequestHeaders.Add("authorization", ApiKey);
                        //set the header to json
                        httpClient.DefaultRequestHeaders.Add("Accepts", "application/json");
                        //send a get request to the transcript enpoint and add the ticketId to the url
                        HttpResponseMessage responseMessage = await httpClient.GetAsync("https://api.assemblyai.com/v2/transcript/" + ticketID);
                        //make sure the call wet through
                        responseMessage.EnsureSuccessStatusCode();
                        //dispaly the data
                        var content = await responseMessage?.Content.ReadAsStringAsync();
                        return Data.Create(content);
                    }
                }
                else
                {
                    Console.WriteLine("Please select 1 or 2 to perform operation");
                    return Data.Create("content", "Please enter valid Keyword like 1, 2 ");
                }
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
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