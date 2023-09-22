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
            Description = "Get the current date and time.";
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override Task<Data?> Process(Information information)
        {
            return Task.FromResult(Data.Create(DateTime.Now.ToString()));
        }
    }
}