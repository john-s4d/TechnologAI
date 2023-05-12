namespace Technologai.Agents
{
    internal class Program
    {
        private static Monitor? _monitor
            ;

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authUri = _config.AuthUri ?? throw new ArgumentNullException(nameof(_config.AuthUri));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            _monitor = new Monitor(authUri, clientId, clientSecret, memberId);

            _monitor.StatusMessage += _monitor_StatusMessage;

            Console.WriteLine("Loading...");

            await _monitor.Start();

            await Program.Run();
            await _monitor.Stop();
        }

        private static void _monitor_StatusMessage(object? sender, string message)        {

            Console.WriteLine($"{_monitor?.Name ?? "Monitor.Local"} {message}");            
        }

        private async static Task Run()
        {
            Console.WriteLine($"{_monitor?.Name} Started");

            do
            {
                //string value = await Task.Run(() =>
                //{
                //    return Console.ReadLine() ?? "";
                //});

                //if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }
                                
            }
            while (true);

        }

    }



}