namespace Technologai.Agents.Core.Coordinator
{
    internal class Program
    {

        private static Coordinator? _coordinator;

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authorityName = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            _coordinator = new Coordinator(authorityName, clientId, clientSecret, memberId);

            _coordinator.StatusMessage += _coordinator_StatusMessage;

            Console.WriteLine("Loading...");

            await _coordinator.Start();
            await Program.Run();
            await _coordinator.Stop();
        }

        private static void _coordinator_StatusMessage(object? sender, string message)        {

            Console.WriteLine($"{_coordinator?.Name ?? "Coordinator.Local"} Status> {message}");            
        }

        private async static Task Run()
        {
            Console.WriteLine($"{_coordinator?.Name} Started. \"quit\" to stop.");

            do
            {
                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? "";
                });

                if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }
                                
            }
            while (true);

        }

    }



}