namespace Technologai
{
    public class Generate32BitString : Process
    {   
        public Generate32BitString() 
        { 
            Name = "Generate 32 Bit String";
            Description =  "Generate a randomized 32-bit string.";
            State = ProcessState.EXECUTE;
        }

        public new string? Execute(in InformationAdapter information)
        {   
            return Utils.GenerateNewIdString(32);
        }
    }
}