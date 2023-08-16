namespace Technologai.Agents.Core.Templates
{
    public class RespondBar : Template
    {   
        public RespondBar() 
        { 
            Id = "respond_bar";
            Description =  "Output the word 'bar'";            
        }
        public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);        

        public override Task<Data?> Process(InformationAdapter information)
        {
            return Task.FromResult((Data?)"bar");
        }
    }
}