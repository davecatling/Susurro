using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SusurroRsa
{
    public class Rsa
    {
        private IComms _comms;

        public Rsa() { }

        private static RSACryptoServiceProvider NewRsa()
        {
            RSACryptoServiceProvider rsa = new()
            {
                KeySize = 2048
            };
            return rsa;
        }

        private static string PublicKeyPath(string username)
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\keys");
            Directory.CreateDirectory(path);
            path = Path.Join(path, $"{username}:public.xml");
            return path;
        }

        public static string PrivateKeyPath(string username)
        {
            var path = PublicKeyPath(username);
            return path[..10] + "private.p8";
        }
        public static string[] CreateKeys(string username, string password, bool overwrite = false)
        {
            var result = new string[2];
            var privatePath = PrivateKeyPath(username);
            if (overwrite != false && File.Exists(privatePath))
                throw new IOException($"Existing file at {privatePath}");
            RSACryptoServiceProvider rsa = NewRsa();
            var keys = rsa.ExportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)password,
                new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 500000)); 
            using BinaryWriter binaryWriter = new(File.Open(privatePath, FileMode.Create));
            binaryWriter.Write(keys);
            result[0] = privatePath;
            var publicKey = rsa.ToXmlString(false);
            var publicPath = PublicKeyPath(username);
            using StreamWriter streamWriter = new(File.Open(publicPath, FileMode.Create));
            streamWriter.Write(publicKey);
            result[1] = publicPath;
            return result;
        }

        private static RSACryptoServiceProvider PublicRsa(string username)
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

            }
            var rsa = NewRsa();
            rsa.FromXmlString(publicKeyXml);
            return rsa;
        }

        private static RSACryptoServiceProvider PrivateRsa(string username, string password)
        {
            var path = PrivateKeyPath(username);
            if (!File.Exists(path))
                throw new IOException($"No file at {path}");
            var rsa = NewRsa();
            byte[] keyBytes;
            using (BinaryReader binaryReader = new(File.Open(path, FileMode.Open)))
            {
                keyBytes = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
            }
            rsa.ImportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)password, keyBytes, out _);
            return rsa;
        }







    }
}
