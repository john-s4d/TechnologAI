using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;
using Technologai;
using Technologai.Agents.Core.DataModels;
using Information = Technologai.Information;

namespace Core.Templates.Jira
{
    /*
    //Update Jira Ticket
    UpdateJiraTicket updateJiraTicket = new();
    var userEditData = new Dictionary<string, object>()
             {
                 {"domain","your domain"},
                 {"issueID","your issueId" },
                 {"username","your username" },
                 {"password","your password (access_token)"},
             };
    Root userData = new();
    userData.fields.description = "Description";
            userData.fields.status = new Status()
    {
        name = "status",
                description = "Description",
                statusCategory = new StatusCategory()
                {
                    name = "name",
                    colorName = "",
                    key = "key",
                }
            };
    userData.fields.duedate = DateTime.Now.AddDays(10);
            userData.fields.summary = "Summary";
            var userDataJson = JsonConvert.SerializeObject(userData);
    GetJiraComments getJiraComments = new();
    GetJiraTicketById getJiraTicketById = new();
    var getJiraTicketByIdDict = new Dictionary<string, object>()
             {
                 {"domain",(string)userEditData["domain"]},
                 {"issueID",(string)userEditData["issueID"]},
                 {"username",(string)userEditData["username"]},
                 {"password",(string)userEditData["password"]}
             };
    var rr1 = getJiraTicketById.Execute(getJiraTicketByIdDict).Result;
    var deserializedData = (Root)rr1["contents"];
    userEditData.Add("editObject", deserializedData);
            var postJiraCommentResult = updateJiraTicket.Execute(userEditData).Result;
    */

    /// <summary>
    /// Update Jira Ticket
    /// </summary>
    public class UpdateJiraTicket : Template
    {
        internal string Username { get; set; } = string.Empty;
        internal string Password { get; set; } = string.Empty;
        public UpdateJiraTicket(string username, string password)
        {
            Id = "update_jira_ticket";
            Description = "Update Jira Ticket By Id";
            InputKeys = new string[] { "domain", "issueID", "editObject" };
            OutputKeys = new string[] { "contents" };
            Username = username;
            Password = password;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            var domain = (string)information.Input.Structured["domain"];
            var issueID = (string)information.Input.Structured["issueID"];
            var editObject = (string)information.Input.Structured["editObject"];

            var jsonData = JsonConvert.SerializeObject(editObject);
            var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");

            try
            {
                var url = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}/comment");
                // Post comments
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                using var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseResult = await response.Content.ReadAsStringAsync();
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "contents", responseResult } }));
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