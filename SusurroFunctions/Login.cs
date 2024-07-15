using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SusurroDtos;
using SusurroFunctions.Model;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;

namespace SusurroFunctions
{
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string name = req.Query["name"];
            string password = req.Query["password"];
            bool result = false;
            await Task.Run(() => { result = TableOperations.Login(name, password); });
            if (result)
                return new OkObjectResult($"User {name} logged in.");
            else
                return new BadRequestObjectResult($"Login for user {name} failed. " +
                    $"Login for {name} will be unavailable for one minute.");            
        }
    }
}
