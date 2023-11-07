using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Technologai.Templates
{
    public class Generate32ByteString : Template
    {   
        public Generate32ByteString() 
        { 
            Id = "generate_32_byte_string";
            Description =  "Generate a randomized 32-Byte string.";
            OutputKeys = new string[] { "text" };
        }
        public override Task<bool> Assess(Information information) => Task.FromResult(true);        

        public async override Task<Data?> Process(Information information)
        {   
            return Data.Create(Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)));
        }
    }
}