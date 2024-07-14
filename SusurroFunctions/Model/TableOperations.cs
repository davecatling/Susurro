using Azure.Data.Tables;
using SusurroFunctions.Dtos;
using System;
using Azure.Identity;
using Azure;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SusurroFunctions.Model
{
    internal static class TableOperations
    {
        internal static async void PutUser(UserDto user)
        {
            var userEntity = new TableEntity(partitionKey: "users", rowKey: user.Name)
            {
                { "Salt", user.Salt },
                { "PasswordHash", user.PasswordHash }
            };
            var tableClient = TableClient();
            await tableClient.UpsertEntityAsync(userEntity);
        }

        internal static string GetPublicKey(string name)
        {
            var user = GetUser(name);
            if (user == null) return null;
            return user.GetString("PublicKey");
        }

        internal static bool PutPublicKey(string name, string publicKey)
        {
            var user = GetUser(name);
            if (user == null) return false;
            if (user.GetString("PublicKey") != null) return false;
            user.Add("PublicKey", publicKey);
            var tableClient = TableClient();
            tableClient.UpsertEntity(user);
            return true;
        }

        private static TableEntity GetUser(string name)
        {
            var tableClient = TableClient();
            Pageable<TableEntity> queryResults = tableClient.Query<TableEntity>(filter: (e) => e.PartitionKey == "users"
                && e.RowKey == name);
            if (!queryResults.Any()) return null;
            return queryResults.First();
        }

        internal static bool UserExists(string name)
        {
            var queryResults = GetUser(name);
            return queryResults != null;            
        }

        internal static bool PasswordOk(string name, string password)
        {
            var user = GetUser(name);
            if (user == null) return false;
            var salt = user.GetBinary("Salt");
            var hashedPassword = HashAndSalt.GetHash(password, salt);
            var storedPassword = user.GetBinary("PasswordHash");
            return hashedPassword.SequenceEqual(storedPassword);
        }
        
        internal static TableClient TableClient()
        {
            var clientId = Environment.GetEnvironmentVariable("MI_CLIENT_ID");
            return new TableClient(new Uri(Environment.GetEnvironmentVariable("STORAGE_ENDPOINT")),
                "susurrotable", 
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId}));
        }
    }
}
