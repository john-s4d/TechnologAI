namespace Technologai.Templates
{
    /// <summary>
    /// Get current datetime in UTC.
    /// </summary>
    internal class GetCurrentDateTimeUTC : Template
    {
        public GetCurrentDateTimeUTC()
        {
            Id = "get_current_date_time_utc";
            Description = "Get the current date and time in UTC";
            OutputKeys = new string[] { "currentDateWithUTC:DateTime" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            return Data.Create(DateTime.UtcNow.ToString());
        }
    }
}