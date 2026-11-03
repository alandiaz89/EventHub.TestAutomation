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

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
        var error = response.ReadAs<ErrorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(error?.Success, Is.False);
            Assert.That(error?.Error, Is.Not.Empty);
        });
    }

    [TestCase("not-an-email", "Test@1234", TestName = "Register_WithInvalidEmail_Returns400")]
    [TestCase("", "Test@1234", TestName = "Register_WithEmptyEmail_Returns400")]
    [TestCase("valid.user@example.com", "12345", TestName = "Register_WithPasswordShorterThan6_Returns400")]
    [TestCase("valid.user@example.com", "", TestName = "Register_WithEmptyPassword_Returns400")]
    public async Task Register_WithInvalidInput_Returns400(string email, string password)
    {
        var input = new AuthInputBuilder().WithEmail(email).WithPassword(password).Build();

        var response = await Clients.Auth.Register(input);

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
        var error = response.ReadAs<ErrorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(error?.Success, Is.False);
            Assert.That(error?.Error, Is.Not.Empty);
        });
    }
}
