namespace SusurroHttp
{
    using SusurroDtos;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;

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
            var newUserDto = new NewUserDto() { Name = name, Password = password };
            var result = await HttpClient.PostAsJsonAsync<NewUserDto>($"{HttpClient.BaseAddress}CreateUser", 
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

        public async Task<MessageDto> GetMsgAsync(string id)
        {
            HttpResponseMessage result;
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{HttpClient.BaseAddress}GetMsg?id={id}")
            };
            result = await HttpClient.SendAsync(requestMessage);
            if (!result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                throw new Exception($"{result.StatusCode}: {response}");
            }
            var msg = await result.Content.ReadFromJsonAsync<MessageDto>() 
                ?? throw new Exception("Failed to rehydrate message.");
            return msg;
        }

        public async Task<HttpResponseMessage> Login(string name, string password)
        {
            HttpResponseMessage result;
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{HttpClient.BaseAddress}Login")
            };
            var base64HeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(
                $"{name}:{password}"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64HeaderValue);
            result = await HttpClient.SendAsync(requestMessage);            
            return result;       
        }

        public async Task<HttpResponseMessage> SendMsgAsync(List<MessageDto> messages)
        {
            HttpResponseMessage result;
            result = await HttpClient.PostAsJsonAsync(
                $"{HttpClient.BaseAddress}SendMsg", messages);
            return result;
        }

        public async Task<HttpResponseMessage> PutKeyAsync(string key)
        {
            var result = await HttpClient.PostAsync($"{HttpClient.BaseAddress}PutKey",
                new StringContent(key));
            return result;
        }

        public async Task<HttpResponseMessage> PutConIdAsync(string conId)
        {
            var result = await HttpClient.PostAsync($"{HttpClient.BaseAddress}PutConId",
                new StringContent(conId));
            return result;
        }
    }
}
