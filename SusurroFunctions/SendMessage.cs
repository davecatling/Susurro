using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SusurroDtos;
using SusurroFunctions.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                if (!msgDtos.All(dto => dto.From == msgDtos[0].From))
                    throw new Exception("Inconsistant FROM values.");
                var userDetails = UserDetailFactory.GetUserDetails(req.Headers.Authorization);
                if (userDetails == null || msgDtos[0].From != userDetails.Name)
                    throw new Exception("Invalid authorization header.");
                if (!TableOperations.PasswordOk(msgDtos[0].From, userDetails.Password))
                    throw new Exception("Bad username or password");
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
                foreach (var msgResult in sendMsgResults)
                {
                    var recipientConId = TableOperations.GetConnectionId(msgResult.To);
                    await signalRMessages.AddAsync(
                        new SignalRMessage
                        { 
                            ConnectionId = recipientConId,
                            Target = "newMessage",
                            Arguments = [msgResult.Id]
                        });
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Sending message failed: {ex.Message}");
            }
        }
    }
}
