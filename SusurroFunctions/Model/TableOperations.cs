using Azure.Data.Tables;
using SusurroFunctions.Dtos;
using System;
using Azure.Identity;
using Azure;
using System.Linq;

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
            var queryResults = GetUser(name);
            if (!queryResults.Any())
                return null;
            return queryResults.First().GetString("PublicKey");
        }

        internal static bool PutPublicKey(string name, string publicKey)
        {
            var queryResults = GetUser(name);
            if (!queryResults.Any()) return false;
            queryResults.First().GetString("PublicKey");
            if (publicKey != null) return false;
            var tableClient = TableClient();
            tableClient.UpsertEntity(queryResults.First());
            return true;
        }

        private static Pageable<TableEntity> GetUser(string name)
        {
            var tableClient = TableClient();
            Pageable<TableEntity> queryResults = tableClient.Query<TableEntity>(filter: (e) => e.PartitionKey == "users"
                && e.RowKey == name);
            return queryResults;
        }

        internal static bool UserExists(string name)
        {
            var queryResults = GetUser(name);
            return queryResults.Any();            
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
