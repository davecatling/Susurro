
namespace SusurroHttp
{

    public interface IComms
    {
        string? BaseUrl { get; set; }

        Task<HttpResponseMessage> CreateUserAsync(string name, string password);
        Task<string> GetKeyAsync(string name);
        Task<HttpResponseMessage> PutConIdAsync(string conId);
        Task<HttpResponseMessage> PutKeyAsync(string key);
        Task<HttpResponseMessage> LoginAsync(string name, string password);
        Task<HttpResponseMessage> LogoutAsync();
    }
}