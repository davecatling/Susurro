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
using Azure.Data.Tables;
using SusurroDtos;

namespace SusurroFunctions
{
    public static class GetMsg
    {
        [FunctionName("GetMsg")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string id = req.Query["id"];
            var userDetails = UserDetailFactory.GetUserDetails(req.Headers.Authorization);
            if (userDetails == null)
                return new BadRequestObjectResult("Invalid authorization header.");
            TableEntity msg = null;
            await Task.Run(() =>
            {
                msg = TableOperations.GetMsg(id);
            });
            if (msg == null)
                return new NotFoundObjectResult($"Message {id} not found.");
            var to = msg.GetString("To");
            if (!TableOperations.PasswordOk(to, userDetails.Password))
                return new BadRequestObjectResult($"Message retrieval failed.");
            var msgDto = new MessageDto()
            {
                Id = msg.RowKey,
                From = msg.GetString("From"),
                CreateTime = (DateTime)msg.GetDateTime("Created"),
                AllTo = msg.GetString("AllTo"),
                To = msg.GetString("To"),
                Text = msg.GetBinary("Text"),
                Signature = msg.GetBinary("Signature")
            };
            TableOperations.DeleteMsg(msg.RowKey);
            return new OkObjectResult(msgDto);
        }
    }
}
