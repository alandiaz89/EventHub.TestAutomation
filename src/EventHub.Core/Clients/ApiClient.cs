using EventHub.Core.Config;
using RestSharp;

namespace EventHub.Core.Clients;

public sealed class ApiClient : IDisposable
{
    private readonly RestClient _client;

    public ApiClient(ApiConfig config)
    {
        _client = new RestClient(new RestClientOptions(config.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        });
    }

    public Task<RestResponse<T>> Get<T>(string endpoint, IDictionary<string, object?>? query = null) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Get), query);

    public Task<RestResponse<T>> Post<T>(string endpoint, object body) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Post).AddJsonBody(body));

    public Task<RestResponse<T>> Put<T>(string endpoint, object body) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Put).AddJsonBody(body));

    public Task<RestResponse<T>> Delete<T>(string endpoint) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Delete));

    private Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, IDictionary<string, object?>? query = null)
    {
        foreach (var (name, value) in query ?? new Dictionary<string, object?>())
        {
             if (value is not null)
            {
                request.AddQueryParameter(name, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            }    
        }
        return _client.ExecuteAsync<T>(request);
    }

    public void Dispose() => _client.Dispose();
}
