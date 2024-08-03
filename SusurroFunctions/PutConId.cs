using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SusurroDtos;
using SusurroFunctions.Model;
using System.IO;
using System.Threading.Tasks;
namespace SusurroFunctions
{
    public static class PutConId
    {
        [FunctionName("PutConId")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var conId = await new StreamReader(req.Body).ReadToEndAsync();
            var userDetails = UserDetailFactory.GetUserDetails(req.Headers.Authorization);
            if (userDetails == null)
                return new BadRequestObjectResult("Invalid authorization header.");
            if (!TableOperations.PasswordOk(userDetails.Name, userDetails.Password))
                return new BadRequestObjectResult($"Put connection ID failure.");
            var result = TableOperations.PutConId(userDetails.Name, conId);
            if (!result)
                return new BadRequestObjectResult($"Put connection ID failure.");
            return new OkObjectResult("Put connection ID success.");
        }
    }
}
