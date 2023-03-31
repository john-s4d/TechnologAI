using Technologai;

internal class Program
{
    private static Agent? _agent;
    private static AppConfig _config = new AppConfig();

    internal static void Main(string[] args)
    {
        _agent = new Agent(_config.Host);

        _agent.Start();
        _agent.Output += _agent_Output;

        Run().Wait();

        _agent.Stop();
    }

    private static void _agent_Output(object? sender, string message)
    {
        Console.WriteLine($"output:> {message}");
    }

    private static void Input(string message)
    {
        _agent?.Input(message);
    }

    private async static Task Run()
    {
        Console.WriteLine("input:>");
        do
        {
            string value = await Task.Run(() =>
            {
                return Console.ReadLine() ?? "";
            });

            if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }

            Input(value);

            Console.WriteLine($"sent:> {value}");
        }
        while (true);

    }


}