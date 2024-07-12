
namespace SusurroHttp
{
    public interface IComms
    {
        string? BaseUrl { get; set; }

        Task<HttpResponseMessage> CreateUserAsync(string name, string password);
        Task<string> GetKeyAsync(string name);
    }
}