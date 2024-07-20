using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using SusurroDtos;
using SusurroFunctions.Model;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Security.Principal;

namespace SusurroFunctions
{
    public static class SendMsg
    {
        [FunctionName("SendMsg")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var msgDtos = JsonConvert.DeserializeObject<List<MessageDto>>(requestBody);
                if (!TableOperations.PasswordOk(msgDtos[0].From, msgDtos[0].Password))
                {
                    throw new Exception("Bad username or password");
                }
                var storedMsgDtos = await TableOperations.PutMessagesAsync(msgDtos);
                var sendMsgResults = new List<SendResult>();
                foreach (var msg in storedMsgDtos)
                {
                    sendMsgResults.Add(new SendResult()
                    {
                        Id = msg.Id,
                        To = msg.To
                    });
                }
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "newMessage",
                        Arguments = [.. sendMsgResults]
                    });
                return new OkObjectResult(sendMsgResults);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Sending message failed: {ex.Message}");
            }
        }
    }
}
