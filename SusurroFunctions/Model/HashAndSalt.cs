using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SusurroFunctions.Model
{
    public static class HashAndSalt
    {
        public static byte[] GenerateSalt()
        {
            var randomBytes = new byte[20];
            RandomNumberGenerator.Create().GetBytes(randomBytes);
            return randomBytes;
        }

        public static byte[] GetHash(string password, byte[] salt)
        {
            var passWord = Encoding.UTF8.GetBytes(password);
            var saltedValue = passWord.Concat(salt).ToArray();
            return SHA256.HashData(saltedValue);
        }
    }
}
