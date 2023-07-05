namespace Technologai.Agents.Core.Processes
{
    public class Generate32BitString : Neuron
    {   
        public Generate32BitString() 
        { 
            Id = "generate_32_bit_string";
            Description =  "Generate a randomized 32-bit string.";
            //defaultState = ProcessState.EXECUTE;
        }

        public static new string Execute(InformationAdapter information)
        {   
            return Utils.GenerateNewIdString(32);
        }
    }
}