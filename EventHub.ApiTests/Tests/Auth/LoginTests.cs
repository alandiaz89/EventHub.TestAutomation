using Allure.NUnit;
using EventHub.ApiTests.Hooks;
using EventHub.Core.Builders;
using EventHub.Core.Clients;
using EventHub.Core.Models.Common;

namespace EventHub.ApiTests.Tests.Auth;

[TestFixture]
[AllureNUnit]
[Category("API")]
[Category("Auth")]
public class LoginTests : ApiHooks
{
    [Test]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        var input = new AuthInputBuilder().Build();
        var registered = await Clients.Auth.Register(input);
        Assert.That((int)registered.StatusCode, Is.EqualTo(201), "Precondition: registration must succeed");

        var response = await Clients.Auth.Login(input);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Success, Is.True);
            Assert.That(response.Data.Token, Is.Not.Empty);
            Assert.That(response.Data.User?.Email, Is.EqualTo(input.Email));
        });
    }

    [Test]
    public async Task Login_WithWrongPassword_Returns400()
    {
        var input = new AuthInputBuilder().Build();
        var registered = await Clients.Auth.Register(input);
        Assert.That((int)registered.StatusCode, Is.EqualTo(201), "Precondition: registration must succeed");

        var response = await Clients.Auth.Login(new AuthInputBuilder().WithEmail(input.Email).WithPassword("Wrong@1234").Build());

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
        var error = response.ReadAs<ErrorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(error?.Success, Is.False);
            Assert.That(error?.Error, Is.EqualTo("Invalid email or password"));
        });
    }

    [Test]
    public async Task Login_WithUnregisteredEmail_Returns400()
    {
        var response = await Clients.Auth.Login(new AuthInputBuilder().Build());

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
        var error = response.ReadAs<ErrorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(error?.Success, Is.False);
            Assert.That(error?.Error, Is.EqualTo("Invalid email or password"));
        });
    }
}
