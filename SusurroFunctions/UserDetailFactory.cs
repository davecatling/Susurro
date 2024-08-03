using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SusurroFunctions
{
    internal static class UserDetailFactory
    {
        public static UserDetails GetUserDetails(string authHeader)
        {
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                string base64vals = authHeader["Basic ".Length..].Trim();
                var encoding = Encoding.GetEncoding("iso-8859-1");
                var namePassword = encoding.GetString(Convert.FromBase64String(base64vals));
                var colonPos = namePassword.IndexOf(':');
                var name = namePassword[..colonPos];
                var password = namePassword[(colonPos + 1)..];
                return new UserDetails(name, password);
            }
            else
                return null;
        }
    }
    internal class UserDetails(string name, string password)
    {
        public string Name => name;
        public string Password => password;
    }
}
