using HtmlAgilityPack;

namespace Technologai.Templates
{
    // <summary>
    /// Read Web Page
    /// </summary>
    public class ReadWebPage : Template
    {
        public ReadWebPage()
        {
            Id = "read_web_page";
            Description = "Read Web Page to scrape html.";
            InputKeys = new[] { "url", "x-path" };
            OutputKeys = new[] { "HtmlNodes[]:nodes" };
        }
        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                var nodes = await Task.Run(() =>
                {
                    // the URL of the target page
                    string url = ((string)information.Input.Structured["url"]).Trim();
                    var web = new HtmlWeb();

                    // downloading to the target page
                    // and parsing its HTML content
                    var document = web.Load(url);

                    // selecting the HTML nodes of interest  
                    var nodes = document.DocumentNode.SelectNodes($"//*{((string)information.Input.Structured["x-path"]).Trim()}");
                    var list = nodes.ToList();
                    return list;
                });
                if (nodes.Any())
                {
                    return Data.Create("nodes", (IEnumerable<IConvertible>)nodes);
                }
                return Data.Create("Error","No data found!");
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
        }
    }
}