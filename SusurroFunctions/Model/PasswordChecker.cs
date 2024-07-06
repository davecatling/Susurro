using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Web;

namespace SusurroFunctions.Model
{   
    public static class PasswordChecker
    {
        private static HttpClient _httpClient;

        static PasswordChecker()
        {
            var hibpUrl = new Uri(Environment.GetEnvironmentVariable("HibpApiUrl"));
            _httpClient = new HttpClient()
            {
                BaseAddress = hibpUrl
            };
        }

        public static async Task<int> HibpCount(string password)
        {
            const int hashSuffixLen = 35;
            // Send first five bytes of SHA1 hash to HIBP
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashData = SHA1.HashData(passwordBytes);
            var hashString = BitConverter.ToString(hashData).Replace("-", "");
            var hashPrefix = hashString[..5];
            var hibpUrl = new Uri(_httpClient.BaseAddress, $"range/{hashPrefix}");
            // Get hash suffixes from HIBP that match our prefix
            var response = await _httpClient.GetAsync(hibpUrl);
            var content = await response.Content.ReadAsStringAsync();
            var partialMatches = content.Split("\r\n").ToList();
            int result = 0;
            // Return occurance count if match found
            partialMatches.ForEach(pm =>
            {
                var testString = $"{hashPrefix}{pm[..hashSuffixLen]}";
                if (testString == hashString)
                    result = int.Parse(pm[(hashSuffixLen + 1)..]);
            });
            // No match found
            return result;
        }
    }
}
