using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SusurroHttp;
using SusurroRsa;

namespace SusurroTestConsole
{
    internal class TestConsole
    {
        private Http? _http;

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
                }
            }
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
            var rsa = new Rsa(_http!);
            var initialCallResult = await _http!.CreateUserAsync(username, password);
            if (initialCallResult.IsSuccessStatusCode)
            {
                Console.WriteLine($"New user {username} created. Generating keys...");
                var paths = rsa.CreateKeys(username, password);
                Console.WriteLine($"New keys created. Key paths:\n{paths[0]}\n{paths[1]}");
            }
            else
            {
                Console.WriteLine("Creating user failed. Error msg:");
                using var streamReader = new StreamReader(initialCallResult.Content.ReadAsStream());
                Console.WriteLine(streamReader.ReadToEnd());
                return;
            }
            Console.WriteLine("Uploading public key...");
            var publicRsa = await rsa.PublicRsaAsync(username);
            var uploadResult = await _http.PutKeyAsync(username, password, publicRsa.ToXmlString(false));
            if (uploadResult.IsSuccessStatusCode)            
                Console.WriteLine($"Private key uploaded.");
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
