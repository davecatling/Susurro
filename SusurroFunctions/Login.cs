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
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var userDetails = UserDetailFactory.GetUserDetails(req.Headers.Authorization);
            if (userDetails == null)
                return new BadRequestObjectResult("Invalid authorization header.");
            bool result = false;
            await Task.Run(() => { result = TableOperations.Login(userDetails.Name, 
                userDetails.Password); });
            if (result)
            {
                var msgIds = TableOperations.GetMsgIds(userDetails.Name);
                return new OkObjectResult(msgIds);
            }
            else
                return new BadRequestObjectResult($"Login for user {userDetails.Name} failed. " +
                    $"Login for {userDetails.Name} will be unavailable for one minute.");            
        }
    }    
}
