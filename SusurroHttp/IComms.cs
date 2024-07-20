
namespace SusurroHttp
{

    public interface IComms
    {
        string? BaseUrl { get; set; }

        Task<HttpResponseMessage> CreateUserAsync(string name, string password);
        Task<string> GetKeyAsync(string name);
        Task<HttpResponseMessage> PutConIdAsync(string name, string password, string conId);
        Task<HttpResponseMessage> PutKeyAsync(string name, string password, string key);
        Task<HttpResponseMessage> Login(string name, string password);
    }
}