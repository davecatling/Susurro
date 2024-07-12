using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SusurroHttp;

namespace SusurroTestConsole
{
    internal class TestConsole
    {
        private Comms? _http;

        internal void Startup()
        {
            HostApplicationBuilder builder = new HostApplicationBuilder();

            builder.Configuration.Sources.Clear();

            builder.Configuration.AddJsonFile("appsettings.json", false);

            _http = new Comms();
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
            var result = await _http!.CreateUserAsync(elements[1], elements[2]);
            Console.WriteLine(result.IsSuccessStatusCode ? "Success!" : "Failed!");
            Console.WriteLine(await result.Content.ReadAsStringAsync());
        }
    }
}
