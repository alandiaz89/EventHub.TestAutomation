using EventHub.Core.Config;
using RestSharp;

namespace EventHub.Core.Clients;

public sealed class ApiClient : IDisposable
{
    private readonly RestClient _client;
    private readonly List<ApiCall> _calls = [];
    private string? _token;

    public ApiClient(ApiConfig config)
    {
        _client = new RestClient(new RestClientOptions(config.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        });
    }

    public Task<RestResponse<T>> Get<T>(string endpoint, IDictionary<string, object?>? query = null) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Get), query);

    public Task<RestResponse<T>> Post<T>(string endpoint, object body) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Post).AddJsonBody(body));

    public Task<RestResponse<T>> PostRawJson<T>(string endpoint, string jsonText) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Post).AddStringBody(jsonText, DataFormat.Json));

    public Task<RestResponse<T>> Put<T>(string endpoint, object body) => ExecuteAsync<T>(new RestRequest(endpoint, Method.Put).AddJsonBody(body));

    public async Task<RestResponse> Delete(string endpoint) => await ExecuteAsync<object>(new RestRequest(endpoint, Method.Delete));

    public void SetToken(string token) => _token = token;

    /// <summary>Requests sent so far by this client, for failure diagnostics. Never includes the token or auth bodies.</summary>
    public IReadOnlyList<ApiCall> Calls => _calls;

    private async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, IDictionary<string, object?>? query = null)
    {
        foreach (var (name, value) in query ?? new Dictionary<string, object?>())
        {
            if (value is not null)
            {
                request.AddQueryParameter(name, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        if (_token is not null)
        {
            request.AddHeader("Authorization", $"Bearer {_token}");
        }

        var response = await _client.ExecuteAsync<T>(request);
        _calls.Add(ApiCall.From(request, response, _client.BuildUri(request)));
        return response;
    }

    public void Dispose() => _client.Dispose();
}
