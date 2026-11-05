using System.Text.Json;
using RestSharp;

namespace EventHub.Core.Clients;

/// <summary>One request/response pair, kept for failure diagnostics.</summary>
public sealed record ApiCall(string Method, string Url, int StatusCode, string? RequestBody, string? ResponseBody)
{
    private const int MaxBodyLength = 2000;

    public static ApiCall From(RestRequest request, RestResponse response, Uri url)
    {
        // Auth endpoints carry passwords and tokens: never keep their request body or a successful response body.
        var isAuth = request.Resource.TrimStart('/').StartsWith("auth/", StringComparison.OrdinalIgnoreCase);
        var status = (int)response.StatusCode;
        var requestBody = isAuth ? null : Serialize(request.Parameters.OfType<BodyParameter>().FirstOrDefault()?.Value);
        var responseBody = isAuth && status is >= 200 and < 300 ? null : response.Content;

        return new ApiCall(request.Method.ToString().ToUpperInvariant(), url.ToString(), status, Truncate(requestBody), Truncate(responseBody));
    }

    public override string ToString() =>
        $"{Method} {Url} -> {StatusCode}"
        + (RequestBody is null ? "" : $"{Environment.NewLine}  request:  {RequestBody}")
        + (ResponseBody is null ? "" : $"{Environment.NewLine}  response: {ResponseBody}");

    private static string? Serialize(object? body) =>
        body is null or string ? body as string : JsonSerializer.Serialize(body, JsonSerializerOptions.Web);

    private static string? Truncate(string? text) =>
        text is { Length: > MaxBodyLength } ? text[..MaxBodyLength] + "..." : text;
}
