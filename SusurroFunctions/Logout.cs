using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SusurroFunctions.Model;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SusurroFunctions
{
    public static class Logout
    {
        [FunctionName("Logout")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var userDetails = UserDetailFactory.GetUserDetails(req.Headers.Authorization);
            if (userDetails == null)
                return new BadRequestObjectResult("Invalid authorization header.");
            bool result = false;
            await Task.Run(() => { result = TableOperations.Logout(userDetails.Name, 
                userDetails.Password); });
            if (result)
                return new OkObjectResult($"User {userDetails.Name} logged out.");
            else
                return new BadRequestObjectResult($"Logout for user {userDetails.Name} failed.");            
        }
    }    
}
