namespace Technologai
{
    public class RespondBar : Template
    {   
        public RespondBar() 
        { 
            Id = "respond_bar";
            Description =  "Output the word 'bar'";            
        }
        public override Task<bool> Assess(Information information) => Task.FromResult(true);        

        public override Task<Data?> Process(Information information)
        {
            return Task.FromResult((Data?)"bar");
        }
    }
}