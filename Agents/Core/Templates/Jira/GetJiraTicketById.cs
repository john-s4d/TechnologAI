using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Technologai;
using Technologai.Agents.Core.DataModels;

namespace Core.Templates.Jira
{
    /*
    // Get Jira Ticket By Id
    GetJiraTicketById getJiraTicketById = new();
    var getJiraTicketByIdDict = new Dictionary<string, object>()
             {
                 {"domain","your domain"},
                 {"issueID","your issueId" },
                 {"username","your username" },
                 {"password","your password (access_token)"}
             };
    var rr1 = getJiraTicketById.Execute(getJiraTicketByIdDict).Result;
    */

    /// <summary>
    /// Get Jira Ticket by Id
    /// </summary>
    public class GetJiraTicketById : Template
    {
        internal string Username { get; set; } = string.Empty;
        internal string Password { get; set; } = string.Empty;
        public GetJiraTicketById(string username, string password)
        {
            Id = "get_jira_ticket_by_id";
            Description = "Get Jira Ticket By Id";
            InputKeys = new string[] { "domain", "issueID"};
            OutputKeys = new string[] { "contents" };
            Username = username;
            Password = password;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            var domain = information.Input.Structured["domain"];
            var issueID = information.Input.Structured["issueID"];

            HttpClient client = new()
            {
                BaseAddress = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes($"{Username} : {Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            try
            {
                // Get comments
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    var responseResult = await response.Content.ReadAsStringAsync();
                    var deserializedData = JsonConvert.DeserializeObject<Root>(responseResult);
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "contents", deserializedData.ToString() } }));
                }
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", $"unable to find the response result" } }));
            }
            catch (Exception e)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", $"unable to find the comments  '{e.Message}'" } }));
            }
        }
    }
}