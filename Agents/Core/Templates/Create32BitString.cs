namespace Technologai.Agents.Core.Templates
{
    public class Generate32BitString : Template
    {   
        public Generate32BitString() 
        { 
            Id = "generate_32_bit_string";
            Description =  "Generate a randomized 32-bit string.";
            //defaultState = ProcessState.EXECUTE;
        }

        public static new string Process(InformationAdapter information)
        {   
            return Utils.GenerateNewIdString(32);
        }
    }
}