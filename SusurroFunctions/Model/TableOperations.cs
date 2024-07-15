using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using SusurroDtos;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        internal async static Task<List<MessageDto>> PutMessagesAsync(List<MessageDto> messages)
        {
            var result = new List<MessageDto>();
            var tableClient = TableClient();
            foreach (MessageDto message in messages)
            {
                var id = Guid.NewGuid().ToString();
                var msgEntity = new TableEntity(partitionKey: "msgs", rowKey: id)
                {
                    { "From", message.From },
                    { "To", message.To },
                    { "Text", message.Text },
                    { "Signature", message.Signature },
                    { "Created", DateTime.UtcNow }
                };
                message.Id = id;
                await tableClient.UpsertEntityAsync(msgEntity);
                result.Add(message);
            }
            return result;
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
            return PasswordOk(user, password);
        }
        
        internal static bool PasswordOk(TableEntity user, string password)
        {
            var salt = user.GetBinary("Salt");
            var hashedPassword = HashAndSalt.GetHash(password, salt);
            var storedPassword = user.GetBinary("PasswordHash");
            return hashedPassword.SequenceEqual(storedPassword);
        }

        internal static bool Login(string name, string password)
        {
            var user = GetUser(name);
            if (user == null) return false;
            var lastFailed = user.GetDateTime("LoginFail");
            if (lastFailed != null)
            {
                if (DateTime.UtcNow.Subtract((DateTime)lastFailed).TotalMinutes < 1)
                    return false;
            }
            var result = PasswordOk(user, password);
            if (result)
            {
                user.Add("LoginOk", DateTime.UtcNow);
            }
            else
            {
                user.Add("LoginFail", DateTime.UtcNow);
            }
            var tableClient = TableClient();
            tableClient.UpsertEntity(user);
            return result;
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
