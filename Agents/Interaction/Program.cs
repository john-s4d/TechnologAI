
namespace Technologai.Agents.Core.Interaction
{
    internal class Program
    {
        private static Interaction? _agent;
        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authorityName = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));
            try
            {

                Console.WriteLine("Loading...");

                _agent = new Interaction(authorityName, clientId, clientSecret, memberId);

                _agent.StatusMessage += _agent_statusMessage;

                _agent.Abilities.Add(new GetUserInput());
                _agent.Abilities.Add(new InteractWithUser());
                var showUserOutput = new ShowUserOutput();
                showUserOutput.OutputMessage += showUserOutput_OutputMessage;
                _agent.Abilities.Add(showUserOutput);

                await _agent.Start();

                //var information = await _agent.Create("get_user_input", $"hello").Publish();

                //var information = await _agent.Create("show_user_output", $"hello").Publish();


                _agent.PublishWithCallback(_agent.Create("interact_with_user", "<Interaction Started>"), information_OnPublishedCallback);

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

            }
            finally
            {
                do
                {
                    Thread.Sleep(1000);
                } while (true);
            }



            /*
            do
            {
                information = await information.Spawn("interact_with_user").Publish();

            } while (!information.Output?.Equals("quit", StringComparison.OrdinalIgnoreCase) ?? true);
            */

        }

        private static void information_OnPublishedCallback(InformationAdapter information)
        {
            //_agent?.PublishWithCallback(_agent.Create("interact_with_user", $"<Interaction Started>"), information_OnPublished);

            Console.WriteLine($"{_agent?.Name} OnPublished> {information.Output}");
        }

        private static void showUserOutput_OutputMessage(string message)
        {
            Console.WriteLine($"{_agent?.Name} Output> {message}");
        }

        private static void _agent_statusMessage(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Interaction.Local"} Status> {message}");
        }


    }
}