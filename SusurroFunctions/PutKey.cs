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
using SusurroDtos;
namespace SusurroFunctions
{
    public static class PutKey
    {
        [FunctionName("PutKey")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var key = await new StreamReader(req.Body).ReadToEndAsync();
            var userDetails = UserDetailFactory.GetUserDetails(req.Headers.Authorization);
            if (userDetails == null)
                return new BadRequestObjectResult("Invalid authorization header.");
            if (!TableOperations.PasswordOk(userDetails.Name, userDetails.Password))
                return new BadRequestObjectResult($"Put key failure.");
            var result = TableOperations.PutPublicKey(userDetails.Name, key);
            if (!result)
                return new BadRequestObjectResult($"Put key failure.");
            return new OkObjectResult("Put key success.");
        }
    }
}
