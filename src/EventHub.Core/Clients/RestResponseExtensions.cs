using System.Text.Json;
using RestSharp;

namespace EventHub.Core.Clients;

public static class RestResponseExtensions
{
    /// <summary>Reads the raw response body as <typeparamref name="T"/>, e.g. an error body on a non-2xx response.</summary>
    public static T? ReadAs<T>(this RestResponse response) =>
        string.IsNullOrWhiteSpace(response.Content)
            ? default
            : JsonSerializer.Deserialize<T>(response.Content, JsonSerializerOptions.Web);
}
