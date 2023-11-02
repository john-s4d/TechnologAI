using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Technologai.Templates
{
    public class Generate32BitString : Template
    {   
        public Generate32BitString() 
        { 
            Id = "generate_32_bit_string";
            Description =  "Generate a randomized 32-bit string.";
            OutputKeys = new string[] { "text" };
        }
        public override Task<bool> Assess(Information information) => Task.FromResult(true);        

        public async override Task<Data?> Process(Information information)
        {
            //return Task.FromResult((Data?)
            //    Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32))
            //    );
            return Data.Create(Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)));
        }
    }
}