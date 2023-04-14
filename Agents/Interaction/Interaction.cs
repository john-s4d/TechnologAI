namespace Technologai.Agents.Core.Interaction
{
    internal class Interaction : TechnologaiAgent
    {
        public Interaction(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        {
            // TODO: This ability should be stored on ChatGPT Member and advised after joining agency.
            Abilities.Add(
                new Ability()
                {
                    Name = "choose_agency_ability",
                    SampleJsonOut = "{\"name\":\"string\"}",
                    Description = "Choose an agency-wide ability to use for the response.",
                    MemberId = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q"
                }
            );

            Abilities.Add(
               new Ability()
               {
                   Name = "chatgpt_prompt",
                   Description = "Send a prompt to ChatGPT and receive a response.",                   
                   MemberId = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q"
               }
           );

            Abilities.Add(
                new Ability()
                {
                    Name = "get_user_input",
                    Description = "Ask the user for information.",
                    SampleJsonIn = "{\"prompt\":\"string\"}",
                    SampleJsonOut = "{\"response\":\"string\"}",
                    MemberId = Identity.Id
                }
            );
        }

        public class choose_agency_ability_response
        {
            public string? name { get; set; }

        }

        public override async Task Execute(Ability ability, InformationHandler information)
        {
            Console.WriteLine($"{Name} Execute> {information.ContextId} | {information.AbilityName} | {information.Input}");

            if (ability.Name == "get_user_input")
            {
                Console.WriteLine(information.Input);
                await Task.Run(async () =>
                {
                    await Receive(information.Close(Console.ReadLine()));
                });
            }
        }

        public async override Task Compile(InformationHandler information)
        {
            Console.WriteLine($"{Name} Compile> {information.ContextId} | {information.Input} | {information.Output}");

            if (information.AbilityName == "get_user_input")
            {
                await information.Spawn("choose_agency_ability", information.Output).Publish();
            }

            if (information.AbilityName == "choose_agency_ability")
            {
                var chosenAbilityName = information.DeserializeOutput<choose_agency_ability_response>()?.name ?? string.Empty;
                await information.Spawn(chosenAbilityName, information.Input).Publish();
            }
        }

        public override Task Review(InformationHandler information)
        {
            Console.WriteLine($"{Name} Review> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }
    }
}
