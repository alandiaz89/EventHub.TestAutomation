using EventHub.Core.Models.Auth;
using RestSharp;

namespace EventHub.Core.Clients;

public sealed class AuthClient(ApiClient api)
{
    public Task<RestResponse<AuthResponse>> Register(AuthInput input) =>
        api.Post<AuthResponse>("auth/register", input);

    public Task<RestResponse<AuthResponse>> Login(AuthInput input) =>
        api.Post<AuthResponse>("auth/login", input);
}
