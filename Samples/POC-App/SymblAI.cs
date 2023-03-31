using Microsoft.Extensions.Configuration;
using SymblAISharp.Async.JobApi;
using SymblAISharp.Async.VideoApi;
using SymblAISharp.Authentication;
using SymblAISharp.Conversation;
using SymblAISharp.Conversation.FormattedTranscript;
using SymblAISharp.Conversation.Conversation;

namespace TestApp
{
    internal class SymblAI
    {
        private readonly IConfiguration _config;
        private string _authToken;
        private DateTime _authExpiry;
        private IVideoApi _videoApi;
        private IJobApi _jobApi;
        private IConversationApi _conversationApi;

        internal SymblAI()
        {
            _config = new ConfigurationBuilder()
           .AddUserSecrets<SymblAI>()
           .Build();

            Authenticate();

            _videoApi = new VideoApi(_authToken);
            _jobApi = new JobApi(_authToken);
            _conversationApi = new ConversationApi(_authToken);

        }

        internal void Authenticate()
        {
            var response = new AuthenticationApi().GetAuthToken(new AuthRequest()
            {
                appId = _config["SymblAIOptions:AppId"],
                appSecret = _config["SymblAIOptions:AppSecret"],
                type = "application"

            });

            _authToken = response.accessToken;
            _authExpiry = DateTime.Now.AddSeconds(response.expiresIn);
        }

        internal async Task<string> GetJobStatus(string jobId)
        {
            var res = await _jobApi.GetJobResponse(jobId);
            return $"status: {res.status}";
        }

        internal void SaveTranscript()
        {
            string conversationId = "<conversationId>";

            var req = new TranscriptRequest()
            {
                contentType = "text/markdown",
                createParagraphs = true,
                showSpeakerSeparation = true,
                phrases = new Phrases()
                {
                    highlightAllKeyPhrases = true
                }
            };
            var response = _conversationApi.GetTranscriptResponse(conversationId, req);

            string filePath = "<filepath>";

            File.WriteAllText(filePath, response.transcript.payload);            
        }
               

        internal async Task<string> SubmitVideoAsync()
        {
            var req = new VideoRequest()
            {
                name = "FL3",
                enableSpeakerDiarization = true,
                detectPhrases = true
            };

            string filePath = "<filepath>";
            //var res =  await _videoApi.PostVideo(File.ReadAllBytes(filePath), req);

            var res = await _videoApi.PutVideo("<conversationId>", File.ReadAllBytes(filePath), req);

            return $"jobId: {res.jobId}, conversationId: {res.conversationId}";
        }
    }
}
