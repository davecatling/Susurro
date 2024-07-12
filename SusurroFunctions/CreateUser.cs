using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SusurroFunctions.Dtos;
using SusurroFunctions.Model;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;

namespace SusurroFunctions
{
    public static class CreateUser
    {
        [FunctionName("CreateUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {                
                var errorMsg = new StringBuilder();
                // JSON payload of user details expected
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var userDto = JsonConvert.DeserializeObject<NewUserDto>(requestBody);
                if (userDto.Name?.Length == 0 || userDto.Password?.Length == 0)
                    // Reject if either username or password missing
                    return new BadRequestObjectResult("Username and password are required");
                // Return if user already exists
                if (TableOperations.UserExists(userDto.Name))
                    return new BadRequestObjectResult($"Username {userDto.Name} is not available");
                // Verify password for complexity and HIBP appearance
                if (!PasswordChecker.Complexity(userDto.Password))
                    errorMsg.AppendLine("Passwords must have at least eight characters, a mix of upper and "
                        + "lowercase characters, special characters and numbers.");
                var hibpCount = await PasswordChecker.HibpCount(userDto.Password);
                if (hibpCount != 0)
                    errorMsg.AppendLine($"Your password appears {hibpCount} time{(hibpCount > 1 ? "s" : "")} " +
                        $"in the Have I Been Pwned database of known breaches.");
                // Return error msg if password validation failed
                if (errorMsg.ToString().Length > 0)
                    return new BadRequestObjectResult($"{errorMsg.ToString().Trim()}");
                // Generate salt and hash password
                var salt = HashAndSalt.GenerateSalt();
                var passwordHash = HashAndSalt.GetHash(userDto.Password, salt);
                // Create and store user details
                var user = new UserDto()
                {
                    Name = userDto.Name,
                    Salt = salt,
                    PasswordHash = passwordHash
                };
                TableOperations.PutUser(user);                
                return new OkObjectResult($"Details for new user {userDto.Name} received");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Creating user failed: {ex.Message}");
            }
        }
    }
}
