namespace Technologai.Templates
{
    /// <summary>
    /// Get current datetime.
    /// </summary>
    internal class GetCurrentDateTime : Template
    {
        public string Description { get; } = "Get current datetime.";
        public string SampleJsonIn { get; } = "{\"returnDateTimeFormat\":\"string\", \"getUtcTime\":\"bool\"}";
        public string SampleJsonOut { get; } = "{\"currentDateTime\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var dateTime = await Task.Run(() =>
            {
                DateTime currentDateTime;
                data.TryGetValue("getUtcTime", out object getUtcTime);
                if (getUtcTime == null || (getUtcTime != null && (bool)getUtcTime == false))
                {
                    currentDateTime = DateTime.Now;
                }
                else
                {
                    currentDateTime = DateTime.UtcNow;
                }

                string dateTimeFormat;
                data.TryGetValue("returnDateTimeFormat", out object returnDateTimeFormat);
                if (returnDateTimeFormat == null || (returnDateTimeFormat != null && string.IsNullOrEmpty((string)returnDateTimeFormat)))
                {
                    return currentDateTime.ToString();
                }
                else
                {
                    dateTimeFormat = returnDateTimeFormat != null ? (string)returnDateTimeFormat : string.Empty;
                    return currentDateTime.ToString(dateTimeFormat);
                }
            });

            return new Dictionary<string, object>() { { "currentDateTime", dateTime } };
        }
    }
}