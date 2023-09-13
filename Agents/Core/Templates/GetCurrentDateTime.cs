using System;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Technologai.Templates
{
    /// <summary>
    /// Get current datetime.
    /// </summary>
    internal class GetCurrentDateTime : Template
    {
        public GetCurrentDateTime()
        {
            Id = "get_current_date_time";
            Description = "Getting Current Date Time";
            InputKeys = new[] { "returnDateTimeFormat", "getUtcTime" };
            OutputKeys = new[] { "currentDateTime" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                var dateTime = await Task.Run(() =>
                {
                    DateTime currentDateTime;
                    var getUtcTime = information.Input.Structured["getUtcTime"].ToString() ?? string.Empty;
                    if (getUtcTime == null || (getUtcTime != null && Convert.ToBoolean(getUtcTime) == false))
                    {
                        currentDateTime = DateTime.Now;
                    }
                    else
                    {
                        currentDateTime = DateTime.UtcNow;
                    }

                    string dateTimeFormat;
                    var returnDateTimeFormat = information.Input.Structured?["returnDateTimeFormat"] ?? string.Empty;
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

                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "returnDateTimeFormat", dateTime } }));
            }
            catch (Exception ex) 
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", ex.Message } }));
            }
            
        }
    }
}