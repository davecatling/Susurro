using Azure.Data.Tables;
using SusurroFunctions.Dtos;
using System;
using Azure.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SusurroFunctions.Model
{
    internal static class TableOperations
    {
        internal static async void PutUser(User user)
        {
            var userEntity = new TableEntity(partitionKey: "Users", rowKey: user.Name)
            {
                { "Salt", user.Salt },
                { "PasswordHash", user.PasswordHash }
            };
            var tableClient = TableClient();
            await tableClient.UpsertEntityAsync(userEntity);
        }

        internal static TableClient TableClient()
        {
            var clientId = "4af8231f-41c5-4283-8583-22074bad5993";
            return new TableClient(new Uri(Environment.GetEnvironmentVariable("STORAGE_ENDPOINT")),
                "susurrotable", 
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId}));
        }
    }
}
