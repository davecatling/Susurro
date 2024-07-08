using Azure.Data.Tables;
using SusurroFunctions.Dtos;
using System;
using Azure.Identity;
using Azure;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace SusurroFunctions.Model
{
    internal static class TableOperations
    {
        internal static async void PutUser(User user)
        {
            var userEntity = new TableEntity(partitionKey: "users", rowKey: user.Name)
            {
                { "Salt", user.Salt },
                { "PasswordHash", user.PasswordHash }
            };
            var tableClient = TableClient();
            await tableClient.UpsertEntityAsync(userEntity);
        }

        internal static bool UserExists(string username)
        {
            var tableClient = TableClient();
            Pageable<TableEntity> queryResults = tableClient.Query<TableEntity>(filter: (e) => e.PartitionKey == "users"
                && e.RowKey == username);
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
