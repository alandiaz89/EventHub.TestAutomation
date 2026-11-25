using EventHub.Core.Models.Auth;

namespace EventHub.Core.Builders;

public sealed class AuthRequestBuilder
{
    private string _email = $"api.user.{Guid.NewGuid():N}@example.com";
    private string _password = Guid.NewGuid().ToString("N")[..6];

    public AuthRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public AuthRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public AuthRequest Build() => new(_email, _password);
}
