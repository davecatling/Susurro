namespace SusurroHttp
{
    using SusurroDtos;
    using System.Net.Http.Json;

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
            var result = await HttpClient.PostAsJsonAsync<NewUser>($"{HttpClient.BaseAddress}CreateUser", newUserDto);
            return result;
        }

        public async Task<string> GetKeyAsync(string name)
        {
            var result = await HttpClient.GetStringAsync($"{HttpClient.BaseAddress}GetKey?name={name}");
            return result;
        }
    }
}
