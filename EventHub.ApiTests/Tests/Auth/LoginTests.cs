using System.Text.Json;
using EventHub.ApiTests.Assertions;
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
    public async Task Me_WithValidToken_Returns200AndUser()
    {
        var input = new AuthInputBuilder().Build();
        var registered = await Clients.Auth.Register(input);
        Assert.That((int)registered.StatusCode, Is.EqualTo(201), "Precondition: registration must succeed");
        await LoginAs(input);

        var response = await Clients.Auth.Me();

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Success, Is.True);
            Assert.That(response.Data.User?.UserId, Is.EqualTo(registered.Data!.User!.Id));
            Assert.That(response.Data.User?.Email, Is.EqualTo(input.Email));
        });

        Assert.That(response.ContentType, Does.StartWith("application/json"));
        using var json = JsonDocument.Parse(response.Content!);
        var user = json.RootElement.GetProperty("user");
        // iat and exp are not in the spec but are returned (JWT issued-at and expiry, epoch seconds).
        string[] numberFields = ["userId", "iat", "exp"];
        string[] stringFields = ["email"];

        ApiAssert.JsonFieldTypes(json.RootElement, JsonValueKind.True, ["success"]);
        ApiAssert.JsonFieldTypes(user, JsonValueKind.Number, numberFields);
        ApiAssert.JsonFieldTypes(user, JsonValueKind.String, stringFields);
    }

    [Test]
    public async Task Me_WithMissingToken_Returns401()
    {
        var response = await Clients.Auth.Me();

        ApiAssert.Error(response, 401, "Unauthorized");
    }

    [Test]
    public async Task Me_WithInvalidToken_Returns401()
    {
        UseToken("invalid.token.value");

        var response = await Clients.Auth.Me();

        ApiAssert.Error(response, 401, "Invalid or expired token");
    }

    [Test]
    public async Task Login_WithWrongPassword_Returns400()
    {
        var input = new AuthInputBuilder().Build();
        var registered = await Clients.Auth.Register(input);
        Assert.That((int)registered.StatusCode, Is.EqualTo(201), "Precondition: registration must succeed");

        var response = await Clients.Auth.Login(new AuthInputBuilder().WithEmail(input.Email).WithPassword("Wrong@1234").Build());

        ApiAssert.Error(response, 400, "Invalid email or password");
    }

    [Test]
    public async Task Login_WithUnregisteredEmail_Returns400()
    {
        var response = await Clients.Auth.Login(new AuthInputBuilder().Build());

        ApiAssert.Error(response, 400, "Invalid email or password");
    }
}
