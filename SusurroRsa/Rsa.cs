using SusurroHttp;
using System.Security.Cryptography;
using System.Text;

namespace SusurroRsa
{
    public class Rsa(IComms http)
    {
        private readonly IComms _http = http;

        private static RSACryptoServiceProvider NewServiceProvider()
        {
            RSACryptoServiceProvider cryptoServiceProvider = new()
            {
                KeySize = 2048
            };
            return cryptoServiceProvider;
        }

        private static string PublicKeyPath(string username)
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Susurro\keys");
            Directory.CreateDirectory(path);
            path = Path.Join(path, $"{username}-public.xml");
            return path;
        }

        public static string PrivateKeyPath(string username)
        {
            var path = PublicKeyPath(username);
            path = string.Concat(path.AsSpan(0, path.Length - 10), "private.p8");
            return path;
        }

        public static string[] CreateKeys(string username, string password, bool overwrite = false)
        {
            var result = new string[2];
            var privatePath = PrivateKeyPath(username);
            if (overwrite != false && File.Exists(privatePath))
                throw new IOException($"Existing file at {privatePath}");
            var cryptoServiceProvider = NewServiceProvider();
            var keys = cryptoServiceProvider.ExportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)password,
                new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 500000));
            using BinaryWriter binaryWriter = new(File.Open(privatePath, FileMode.Create));
            binaryWriter.Write(keys);
            result[0] = privatePath;
            var publicKey = cryptoServiceProvider.ToXmlString(false);
            var publicPath = PublicKeyPath(username);
            using StreamWriter streamWriter = new(File.Open(publicPath, FileMode.Create));
            streamWriter.Write(publicKey);
            result[1] = publicPath;
            return result;
        }

        public async Task<RSACryptoServiceProvider> PublicRsaAsync(string username)
        {
            string publicKeyXml;
            var path = PublicKeyPath(username);
            if (File.Exists(path))
            {
                using StreamReader streamReader = new(File.Open(path, FileMode.Open));
                publicKeyXml = streamReader.ReadToEnd();
            }
            else
            {
                publicKeyXml = await _http.GetKeyAsync(username);
                using StreamWriter streamWriter = new(File.Open(path, FileMode.Create));
                streamWriter.Write(publicKeyXml);
            }
            var cryptoServiceProvider = NewServiceProvider();
            cryptoServiceProvider.FromXmlString(publicKeyXml);
            return cryptoServiceProvider;
        }

        private static RSACryptoServiceProvider PrivateRsa(string username, string password)
        {
            var path = PrivateKeyPath(username);
            if (!File.Exists(path))
                throw new IOException($"No file at {path}");
            var cryptoServiceProvider = NewServiceProvider();
            byte[] keyBytes;
            using (BinaryReader binaryReader = new(File.Open(path, FileMode.Open)))
            {
                keyBytes = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
            }
            cryptoServiceProvider.ImportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)password, keyBytes, out _);
            return cryptoServiceProvider;
        }

        public async Task<byte[]> EncryptAsync(string plainString, string to)
        {
            byte[] encryptedBytes;
            using (var cryptoServiceProvider = await PublicRsaAsync(to))
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainString);
                encryptedBytes = cryptoServiceProvider.Encrypt(plainBytes, true);
            }
            return encryptedBytes;
        }

        public static string Decrypt(byte[] encryptedBytes, string username, string password)
        {
            byte[] decryptedBytes;
            using (var cryptoServiceProvider = PrivateRsa(username, password))
            {
                decryptedBytes = cryptoServiceProvider.Decrypt(encryptedBytes, true);
            }
            return UnicodeEncoding.UTF8.GetString(decryptedBytes);
        }

        public static byte[] Sign(string plain, string from, string password)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plain);
            using var cryptoServiceProvider = PrivateRsa(from, password);
            return cryptoServiceProvider.SignData(plainBytes, SHA256.Create());
        }

        public async Task<bool> SignatureOkAsync(byte[] signature, string plain, string from)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plain);
            using var rsa = await PublicRsaAsync(from);
            return rsa.VerifyData(plainBytes, SHA256.Create(), signature!);
        }
    }
}
