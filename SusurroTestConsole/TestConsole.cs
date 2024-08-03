using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SusurroHttp;
using SusurroRsa;
using SusurroDtos;
using SusurroSignalR;
using System.Security.Cryptography;
using System.Linq;
using System.Net.Http.Json;

namespace SusurroTestConsole
{
    internal class TestConsole
    {
        private Http? _http;
        private SignalR? _signalR;
        private string _username;
        private string _password;

        internal void Startup()
        {
            HostApplicationBuilder builder = new HostApplicationBuilder();

            builder.Configuration.Sources.Clear();

            builder.Configuration.AddJsonFile("appsettings.json", false);

            _http = new Http();
            builder.Configuration.GetSection("Http").Bind(_http);

            var command = string.Empty;

            while (!command.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                command = Console.ReadLine();
                if (command == null)
                    break;
                var elements = command.Split(' ');
                switch (elements[0].ToLower())
                {
                    case "createuser":
                        {
                            CreateUser(elements);
                            break;
                        }
                    case "getpublickey":
                        {
                            GetPublicKey(elements); 
                            break;
                        }
                    case "login":
                        {
                            Login(elements); 
                            break;
                        }
                    case "sendmsg":
                        {
                            SendMsgAsync(elements); 
                            break;
                        }
                    case "getmsg":
                        {
                            GetMsgAsync(elements); 
                            break;
                        }
                }
            }
        }

        internal async void GetMsgAsync(string[] elements)
        {
            if (elements.Length < 2)
            {
                Console.WriteLine("Expected: getmsg <msgId>");
                return;
            }
            var msgDto = await _http!.GetMsgAsync(elements[1]);
            var msgText = Rsa.Decrypt(msgDto.Text, _username, _password);
            var signatureOk = await new Rsa(_http!).SignatureOkAsync(msgDto.Signature, msgText, msgDto.From);
            if (!signatureOk)
            {
                Console.WriteLine("Message signature check failed!");
                return;
            }
            Console.WriteLine($"----\nFrom: {msgDto.From}\nTo: {msgDto.AllTo}" +
                $"\nSent: {msgDto.CreateTime.ToLocalTime():dd-MM-yyyy HH:mm}\n{msgText}\n----");
        }

        internal async void SendMsgAsync(string[] elements)
        {
            if (elements.Length < 2)
            {
                Console.WriteLine("Expected: sendmsg <recipient1name> <recipient2name>...");
                return;
            }
            if (_username == null)
            {
                Console.WriteLine("No logged in user. Please log in and try again.");
                return;
            }
            Console.WriteLine("Enter message text:");
            var plainText = Console.ReadLine();
            var messages = new List<MessageDto>();
            bool exceptionOccured = false;
            string allTo = string.Empty;
            for (var i = 1; i < elements.Length; i++)
            {
                allTo += $"{elements[i]} ";
                var to = elements[i];
                byte[]? cypherText = null;
                byte[]? signature = null;
                try
                {
                    cypherText = await new Rsa(_http!).EncryptAsync(plainText!, to);
                    signature = Rsa.Sign(plainText!, _username, _password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sending failed for user {to}: {ex.Message}");
                    exceptionOccured = true;
                }
                if (!exceptionOccured)
                {
                    messages.Add(new MessageDto()
                    {
                        From = _username,
                        To = to,
                        Text = cypherText!,
                        Signature = signature!
                    });
                }
            }
            allTo = allTo[..^1];
            messages.ForEach(m => m.AllTo = allTo);
            var result = await _http!.SendMsgAsync(messages);
            if (result.IsSuccessStatusCode)
            {
                Console.WriteLine("Message sent.");
            }
            else
            {
                var errorText = await result.Content.ReadAsStringAsync();
                Console.WriteLine($"Sending message failed: {errorText}");
            }
        }

        internal async void Login(string[] elements)
        {
            if (elements.Length < 2)
            {
                Console.WriteLine("Expected: login <name> <password>");
                return;
            }
            var name = elements[1];
            var password = elements[2];
            var result = await _http!.Login(name, password);
            if (result.IsSuccessStatusCode)
            {
                Console.WriteLine($"User {name} logged in.");
                _username = name;
                _password = password;
                _signalR = new SignalR(name, password, _http);
                _signalR.ConnectAsync();
                _signalR.MsgIdReceived += SignalRmsgIdReceived;
            }
            else
            {
                using var streamReader = new StreamReader(result.Content.ReadAsStream());
                Console.WriteLine(streamReader.ReadToEnd());
            }
        }

        private void SignalRmsgIdReceived(object sender, MsgIdReceivedEventArgs e)
        {
            GetMsgAsync(["getmsg", e.Id]);
        }

        internal async void GetPublicKey(string[] elements)
        {
            if (elements.Length < 2)
            {
                Console.WriteLine("Expected: getpublickey <name>");
                return;
            }
            var username = elements[1];
            Console.WriteLine($"Getting public key for {username}...");
            RSACryptoServiceProvider cryptoServiceProvider;
            try
            {
                cryptoServiceProvider = await new Rsa(_http!).PublicRsaAsync(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
            if (cryptoServiceProvider != null)
                Console.WriteLine(cryptoServiceProvider.ToXmlString(false));
        }

        internal async void CreateUser(string[] elements)
        {
            if (elements.Length < 3)
            {
                Console.WriteLine("Expected: createuser <name> <password>");
                return;
            }
            var username = elements[1];
            var password = elements[2];
            var initialCallResult = await _http!.CreateUserAsync(username, password);
            if (initialCallResult.IsSuccessStatusCode)
            {
                Console.WriteLine($"New user {username} created. Generating keys...");
                var paths = Rsa.CreateKeys(username, password);
                Console.WriteLine($"New keys created. Key paths:\n{paths[0]}\n{paths[1]}");
            }
            else
            {
                Console.WriteLine("Creating user failed. Error msg:");
                using var streamReader = new StreamReader(initialCallResult.Content.ReadAsStream());
                Console.WriteLine(streamReader.ReadToEnd());
                return;
            }
            Console.WriteLine("Logging in as new user..");
            Login(["",username, password]);
            Console.WriteLine("Uploading public key...");
            var publicRsa = await new Rsa(_http!).PublicRsaAsync(username);
            var uploadResult = await _http.PutKeyAsync(publicRsa.ToXmlString(false));
            if (uploadResult.IsSuccessStatusCode)            
                Console.WriteLine($"Public key uploaded.");
            else
            {
                Console.WriteLine("Uploading key failed. Error msg:");
                using var streamReader = new StreamReader(uploadResult.Content.ReadAsStream());
                Console.WriteLine(streamReader.ReadToEnd());
                return;
            }
        }
    }
}
