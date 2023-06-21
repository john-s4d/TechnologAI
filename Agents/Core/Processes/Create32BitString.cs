namespace Technologai
{
    public class Generate32BitString : Process
    {   
        public Generate32BitString(ref ProcessState defaultState) 
        { 
            Id = "generate_32_bit_string";
            Description =  "Generate a randomized 32-bit string.";
            defaultState = ProcessState.EXECUTE;
        }

        public static new string Execute(InformationAdapter information)
        {   
            return Utils.GenerateNewIdString(32);
        }
    }
}