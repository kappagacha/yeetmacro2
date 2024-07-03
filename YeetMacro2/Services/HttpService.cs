namespace YeetMacro2.Services;

public interface IHttpService
{
    Task<string> GetAsync(string url, IDictionary<string, string> headers = null);
    Task<Stream> GetStreamAsync(string url);
}

public class HttpService(IHttpClientFactory httpClientFactory) : IHttpService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<string> GetAsync(string url, IDictionary<string, string> headers = null)
    {
        var httpClient = _httpClientFactory.CreateClient();
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        } 
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Stream> GetStreamAsync(string url)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStreamAsync();
    }
}
