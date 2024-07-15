namespace SusurroHttp
{
    using SusurroDtos;
    using System.Net.Http.Json;
    using System.Xml.Linq;

    public class Http : IComms
    {
        private HttpClient? _httpClient;

        public string? BaseUrl { get; set; }

        private HttpClient HttpClient
        {
            get
            {
                _httpClient ??= new HttpClient
                {
                    BaseAddress = new Uri(BaseUrl!)
                };
                return _httpClient;
            }
        }

        public async Task<HttpResponseMessage> CreateUserAsync(string name, string password)
        {
            var newUserDto = new NewUser() { Name = name, Password = password };
            var result = await HttpClient.PostAsJsonAsync<NewUser>($"{HttpClient.BaseAddress}CreateUser", 
                newUserDto);
            return result;
        }

        public async Task<string> GetKeyAsync(string name)
        {
            string result;
            try
            {
                result = await HttpClient.GetStringAsync($"{HttpClient.BaseAddress}GetKey?name={name}");
            }
            catch (HttpRequestException ex) 
            {
                result = ex.Message;
            }
            return result;
        }

        public async Task<HttpResponseMessage> Login(string name, string password)
        {
            HttpResponseMessage result;
            result = await HttpClient.GetAsync(
                    $"{HttpClient.BaseAddress}Login?name={name}&password={password}");
            return result;
        }

        public async Task<HttpResponseMessage> PutKeyAsync(string name, string password, string key)
        {
            var putKeyDto = new PutKeyDto() { Name = name, Password = password, Key = key };
            var result = await HttpClient.PostAsJsonAsync<PutKeyDto>($"{HttpClient.BaseAddress}PutKey",
                putKeyDto);
            return result;
        }
    }
}
