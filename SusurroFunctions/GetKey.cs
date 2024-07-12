using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SusurroFunctions.Model;

namespace SusurroFunctions
{
    public static class GetKey
    {
        [FunctionName("GetKey")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string name = req.Query["name"];
            string key = null;
            await Task.Run(() =>
            {
                key = TableOperations.GetPublicKey(name);
            });
            if (key == null)
                return new NotFoundObjectResult($"User {name} not found.");
            return new OkObjectResult(key);
        }
    }
}
