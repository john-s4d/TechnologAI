namespace Technologai.Agents.Core.DataModels
{
    public class Fields
    {
        public string description { get; set; } = string.Empty;
        public Status status { get; set; } = null!;
        public StatusCategory statusCategory { get; set; } = null!;
        public object duedate { get; set; } = null!;
        public string summary { get; set; } = null!;
    }
    public class Root
    {
        public string expand { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string self { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public Fields fields { get; set; } = new Fields();
    }

    public class Status
    {
        public string self { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string iconUrl { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public StatusCategory statusCategory { get; set; } = new StatusCategory();
    }

    public class StatusCategory
    {
        public string self { get; set; } = string.Empty;
        public int id { get; set; }
        public string key { get; set; } = string.Empty;
        public string colorName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }
}
