using Azure.Data.Tables;
using SusurroFunctions.Dtos;
using System;
using Azure.Identity;

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

        internal static TableClient TableClient()
        {
            var clientId = Environment.GetEnvironmentVariable("MI_CLIENT_ID");
            return new TableClient(new Uri(Environment.GetEnvironmentVariable("STORAGE_ENDPOINT")),
                "susurrotable", 
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId}));
        }
    }
}
