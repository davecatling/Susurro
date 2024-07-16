﻿using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SusurroHttp;

namespace SusurroRsa
{
    public class Rsa
    {
        private IComms _comms;

        public Rsa(IComms comms)
        {
            _comms = comms;
        }

        private RSACryptoServiceProvider NewRsa()
        {
            RSACryptoServiceProvider rsa = new()
            {
                KeySize = 2048
            };
            return rsa;
        }

        private string PublicKeyPath(string username)
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Susurro\keys");
            Directory.CreateDirectory(path);
            path = Path.Join(path, $"{username}-public.xml");
            return path;
        }

        public string PrivateKeyPath(string username)
        {
            var path = PublicKeyPath(username);
            path = string.Concat(path.AsSpan(0, path.Length - 10), "private.p8");
            return path;
        }

        public string[] CreateKeys(string username, string password, bool overwrite = false)
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
                try
                {
                    publicKeyXml = await _comms.GetKeyAsync(username);
                }
                catch (Exception)
                {
                    throw;
                }
                using StreamWriter streamWriter = new(File.Open(path, FileMode.Create));
                streamWriter.Write(publicKeyXml);
            }
            var rsa = NewRsa();
            rsa.FromXmlString(publicKeyXml);
            return rsa;
        }

        private RSACryptoServiceProvider PrivateRsa(string username, string password)
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

        public async Task<byte[]> EncryptAsync(string plainString, string to)
        {
            byte[] encryptedBytes;
            using (var rsa = await PublicRsaAsync(to))
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainString);
                encryptedBytes = rsa.Encrypt(plainBytes, true);
            }
            return encryptedBytes;
        }

        public string Decrypt(byte[] encryptedBytes, string username, string password)
        {
            byte[] decryptedBytes;
            using (var rsa = PrivateRsa(username, password))
            {
                decryptedBytes = rsa.Decrypt(encryptedBytes, true);
            }
            return UnicodeEncoding.UTF8.GetString(decryptedBytes);
        }

        public byte[] Sign(string plain, string from, string password)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plain);
            using var rsa = PrivateRsa(from, password);
            return rsa.SignData(plainBytes, SHA256.Create());
        }

        public async Task<bool> SignatureOkAsync(byte[] signature, string plain, string from)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plain);
            using var rsa = await PublicRsaAsync(from);
            return rsa.VerifyData(plainBytes, SHA256.Create(), signature!);
        }
    }
}
