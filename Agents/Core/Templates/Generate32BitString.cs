namespace Technologai
{
    public class Generate32BitString : Template
    {   
        public Generate32BitString() 
        { 
            Id = "generate_32_bit_string";
            Description =  "Generate a randomized 32-bit string.";            
        }
        public override Task<bool> Assess(Information information) => Task.FromResult(true);        

        public override Task<Data?> Process(Information information)
        {
            return Task.FromResult((Data?)new Data(Utils.GenerateNewIdString(32)));
        }
    }
}