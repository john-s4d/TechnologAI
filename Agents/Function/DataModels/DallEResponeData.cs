namespace Technologai.Agents.Core.DataModels
{
    public class DallEResponseData
    {
        public int created { get; set; }
        public List<ImageUrl> data { get; set; } = new List<ImageUrl>();
    }
    public class ImageUrl
    {
        public string url { get; set; } = null!;
    }

}
