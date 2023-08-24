using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;
using Technologai.Agents.Core.DataModels;

namespace Technologai.Templates
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
        public string Description { get; } = "Update Jira Ticket By Id";
        public string SampleJsonIn { get; set; } = "{\"domain\":\"string\",\"issueID\":\"string\",\"username\":\"string\",\"password\":\"string\",\"editObject\":\"RootModel\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var domain = ((string)data["domain"]);
            var issueID = ((string)data["issueID"]);
            var username = ((string)data["username"]);
            var password = ((string)data["password"]);
            var editObject = ((Root)data["editObject"]);

            var jsonData = JsonConvert.SerializeObject(editObject);
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");

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
                    return new Dictionary<string, object> { { "contents", responseResult } };
                }
                return new Dictionary<string, object> { { "Error", $"unable to find the response result" } }; ;
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "Error", $"unable to find the comments  '{e.Message}'" } };
            }
        }
    }
}