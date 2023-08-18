using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

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
            return Task.FromResult((Data?)
                Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32))
                );
        }
    }
}