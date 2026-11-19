using EventHub.Core.Http;
using EventHub.Core.Models.Auth;
using RestSharp;

namespace EventHub.Core.Services;

public sealed class AuthService(ApiClient api)
{
    public Task<RestResponse<AuthResponse>> Register(AuthInput input) =>
        api.Post<AuthResponse>("auth/register", input);

    public Task<RestResponse<AuthResponse>> Login(AuthInput input) =>
        api.Post<AuthResponse>("auth/login", input);

    public Task<RestResponse<MeResponse>> Me() =>
        api.Get<MeResponse>("auth/me");
}
