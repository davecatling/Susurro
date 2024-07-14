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
using SusurroFunctions.Dtos;
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
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var putKeyDto = JsonConvert.DeserializeObject<PutKeyDto>(requestBody);
            if (!TableOperations.PasswordOk(putKeyDto.Name, putKeyDto.Password))
                return new BadRequestObjectResult($"Put key failure.");
            var result = TableOperations.PutPublicKey(putKeyDto.Name, putKeyDto.Key);
            if (!result)
                return new BadRequestObjectResult($"Put key failure.");
            return new OkObjectResult("Put key success.");
        }
    }
}
