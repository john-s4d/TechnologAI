using HtmlAgilityPack;

namespace Technologai.Templates
{
    // <summary>
    /// Read Web Page
    /// </summary>
    public class ReadWebPage : Template
    {
        public string Description { get; } = "Read Web Page to scrape html.";
        public string SampleJsonIn { get; set; } = "{\"url\":\"string\", \"x-path\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"result\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            try
            {
                var nodes = await Task.Run(() =>
                {
                    // the URL of the target page
                    string url = ((string)data["url"]).Trim();
                    var web = new HtmlWeb();

                    // downloading to the target page
                    // and parsing its HTML content
                    var document = web.Load(url);

                    // selecting the HTML nodes of interest  
                    var nodes = document.DocumentNode.SelectNodes($"//*{((string)data["x-path"]).Trim()}");
                    var list = nodes.ToList();
                    return list;
                });
                if (nodes.Any())
                {
                    return new Dictionary<string, object>() { { "result", nodes } };
                }

                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error occured while processing request: {ex.Message}" } };
            }
        }
    }
}