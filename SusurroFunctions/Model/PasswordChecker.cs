using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SusurroFunctions.Model
{
    public static partial class PasswordChecker
    {
        private static HttpClient _httpClient;

        static PasswordChecker()
        {
            var hibpUrl = new Uri(Environment.GetEnvironmentVariable("HIBP_URL"));
            _httpClient = new HttpClient()
            {
                BaseAddress = hibpUrl
            };
        }

        public static bool Complexity(string password)
        {
            // Check for password length, mix of upper and low case plus numbers
            if (password == null) return false;
            if (password.Length < 8) return false;
            var regex = SpecialChars();
            if (!regex.IsMatch(password)) return false;
            regex = LowerCaseChars();
            if (!regex.IsMatch(password)) return false;
            regex = UpperCaseChars();
            if (!regex.IsMatch(password)) return false;
            regex = Numbers();
            if (!regex.IsMatch(password)) return false;
            return true;
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

        [GeneratedRegex(@"[\!@#$%^&*()\\[\]{}\-_+=~`|:;/""'<>,./?]")]
        private static partial Regex SpecialChars();
        [GeneratedRegex(@"[a-z]")]
        private static partial Regex LowerCaseChars();
        [GeneratedRegex(@"[A-Z]")]
        private static partial Regex UpperCaseChars();
        [GeneratedRegex(@"[0-9]")]
        private static partial Regex Numbers();
    }
}
