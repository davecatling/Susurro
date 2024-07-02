using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SusurroFunctions;
using SusurroFunctions.Dtos;

namespace SusurroFunctions
{
    public static class FunctionsApi
    {
        [FunctionName("CreateUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userDto = JsonConvert.DeserializeObject<User>(requestBody);

            if (userDto == null)
            {
                return new BadRequestObjectResult("New user details invalid");
            }
            
            return new OkObjectResult($"Details for new user {userDto.Name} received");
        }
    }
}
