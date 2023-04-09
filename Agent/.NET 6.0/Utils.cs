using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;


namespace Technologai
{
    public static class Utils
    {
        public static byte[] GenerateNewIdBytes(int size = 32) {
            
            // TODO: Guarantee unique
            return RandomNumberGenerator.GetBytes(size);
        }

        public static string GenerateNewIdString(int size = 32)
        {   
            return Base64UrlEncoder.Encode(GenerateNewIdBytes(size));
        }
    }
}
