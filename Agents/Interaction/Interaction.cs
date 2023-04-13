namespace Technologai.Agents.Core.Interaction
{
    internal class Interaction : TechnologaiAgent
    {
        public Interaction(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        {
            Abilities.Add(
                new Ability()
                {
                    Name = "get_user_input",
                    Description = "Ask the user for information.",
                    SchemaIn = "{\"question\":string}",
                    SchemaOut = "{\"response\":string}"
                }
            );
        }

        public override async Task Execute(Ability ability, InformationHandler information)
        {
            Console.WriteLine($"{Name} Execute> {information.ContextId} | {information.AbilityName} | {information.Input}");

            if (ability.Name == "get_user_input")
            {
                Console.WriteLine(information.Input);
                await Task.Run(() =>
                {
                    information.Close(Console.ReadLine());
                });
            }
        }

        public override Task Handle(InformationHandler information)
        {
            Console.WriteLine($"{Name} Handle> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }
    }
}
