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
public class RegisterTests : ApiHooks
{
    [Test]
    public async Task Register_WithValidData_Returns201AndToken()
    {
        var input = new AuthInputBuilder().Build();

        var response = await Clients.Auth.Register(input);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        Assert.That(response.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Success, Is.True);
            Assert.That(response.Data.Token, Is.Not.Empty);
            Assert.That(response.Data.User?.Email, Is.EqualTo(input.Email));
            Assert.That(response.Data.User?.Id, Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task Register_WithAlreadyRegisteredEmail_Returns400()
    {
        var input = new AuthInputBuilder().Build();
        var first = await Clients.Auth.Register(input);
        Assert.That((int)first.StatusCode, Is.EqualTo(201), "Precondition: first registration must succeed");

        var response = await Clients.Auth.Register(input);

        ApiAssert.Error(response, 400, "Email already registered");
    }

    [TestCase("not-an-email", TestName = "Register_WithInvalidEmailFormat_Returns400")]
    [TestCase("", TestName = "Register_WithEmptyEmail_Returns400")]
    public async Task Register_WithInvalidEmail_Returns400(string email)
    {
        var input = new AuthInputBuilder().WithEmail(email).Build();

        var response = await Clients.Auth.Register(input);

        ApiAssert.ValidationError(response, "email", "A valid email is required", expectedDetailCount: 1);
    }

    [TestCase("12345", TestName = "Register_WithPasswordShorterThan6_Returns400")]
    [TestCase("", TestName = "Register_WithEmptyPassword_Returns400")]
    public async Task Register_WithInvalidPassword_Returns400(string password)
    {
        var input = new AuthInputBuilder().WithPassword(password).Build();

        var response = await Clients.Auth.Register(input);

        ApiAssert.ValidationError(response, "password", "Password must be at least 6 characters", expectedDetailCount: 1);
    }
}
