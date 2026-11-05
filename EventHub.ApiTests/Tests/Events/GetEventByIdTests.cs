using System.Globalization;
using System.Text.Json;
using Allure.NUnit;
using EventHub.ApiTests.Assertions;
using EventHub.ApiTests.Hooks;
using EventHub.Core.Builders;

namespace EventHub.ApiTests.Tests.Events;

[TestFixture]
[AllureNUnit]
[Category("API")]
[Category("Events")]
public class GetEventByIdTests : ApiHooks
{
    [Test]
    public async Task GetEventById_WithExistingEvent_Returns200AndMatchesCreatedData()
    {
        await LoginAsConfiguredUser();
        var input = new CreateEventInputBuilder().WithTotalSeats(25).WithPrice(150).Build();
        var created = (await Clients.Events.CreateEvent(input)).Data?.Data;
        TrackEvent(created);
        Assert.That(created, Is.Not.Null, "Arrange failed: the event was not created");

        var response = await Clients.Events.GetEventById(created!.Id);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        var fetched = response.Data?.Data;
        Assert.That(fetched, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Success, Is.True);
            Assert.That(fetched!.Id, Is.EqualTo(created.Id));
            Assert.That(fetched.Title, Is.EqualTo(input.Title));
            Assert.That(fetched.Description, Is.EqualTo(input.Description));
            Assert.That(fetched.Category, Is.EqualTo(input.Category));
            Assert.That(fetched.Venue, Is.EqualTo(input.Venue));
            Assert.That(fetched.City, Is.EqualTo(input.City));
            Assert.That(fetched.EventDate, Is.EqualTo(input.EventDate).Within(1).Milliseconds);
            Assert.That(decimal.Parse(fetched.Price!, CultureInfo.InvariantCulture), Is.EqualTo(input.Price));
            Assert.That(fetched.TotalSeats, Is.EqualTo(25));
            Assert.That(fetched.AvailableSeats, Is.EqualTo(25));
        });
    }

    // Product bug: the spec says price is a number, but the API returns it as a string ("150").
    [Test]
    [Category("Product-Bug")]
    public async Task GetEventById_WithExistingEvent_ReturnsJsonWithEveryDocumentedField()
    {
        await LoginAsConfiguredUser();
        var created = (await Clients.Events.CreateEvent(new CreateEventInputBuilder().Build())).Data?.Data;
        TrackEvent(created);
        Assert.That(created, Is.Not.Null, "Arrange failed: the event was not created");

        var response = await Clients.Events.GetEventById(created!.Id);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.ContentType, Does.StartWith("application/json"));
        using var json = JsonDocument.Parse(response.Content!);
        var data = json.RootElement.GetProperty("data");
        string[] stringFields = ["title", "description", "category", "venue", "city", "eventDate", "createdAt", "updatedAt"];
        string[] numberFields = ["id", "price", "totalSeats", "availableSeats"];
        string[] nullableStringFields = ["imageUrl"];

        ApiAssert.JsonFieldTypes(json.RootElement, JsonValueKind.True, ["success"]);
        ApiAssert.JsonFieldTypes(data, JsonValueKind.String, stringFields);
        ApiAssert.JsonFieldTypes(data, JsonValueKind.Number, numberFields);
        ApiAssert.JsonFieldTypes(data, JsonValueKind.String, nullableStringFields, nullable: true);
    }

    // Product bug: the spec's Event schema has no userId or isStatic, yet the API returns both,
    // exposing the owner's internal id to every caller.
    [TestCase("userId", TestName = "GetEventById_WithExistingEvent_DoesNotExposeUserId")]
    [TestCase("isStatic", TestName = "GetEventById_WithExistingEvent_DoesNotExposeIsStatic")]
    [Category("Product-Bug")]
    public async Task GetEventById_WithExistingEvent_DoesNotReturnUndocumentedField(string field)
    {
        await LoginAsConfiguredUser();
        var created = (await Clients.Events.CreateEvent(new CreateEventInputBuilder().Build())).Data?.Data;
        TrackEvent(created);
        Assert.That(created, Is.Not.Null, "Arrange failed: the event was not created");

        var response = await Clients.Events.GetEventById(created!.Id);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        using var json = JsonDocument.Parse(response.Content!);
        Assert.That(json.RootElement.GetProperty("data").TryGetProperty(field, out _), Is.False, field);
    }

    [TestCase(999999999, TestName = "GetEventById_WithNonExistentId_Returns404")]
    [TestCase(0, TestName = "GetEventById_WithZeroId_Returns404")]
    [TestCase(-1, TestName = "GetEventById_WithNegativeId_Returns404")]
    public async Task GetEventById_WithUnknownId_Returns404(int id)
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEventById(id);

        ApiAssert.Error(response, 404, $"Event with id {id} not found");
    }

    // Product bug: an id beyond the database integer range makes the API answer 500 "Internal server error".
    // A well-formed id that does not exist should give 404 (as 2147483648 does). Expected to fail until fixed.
    [Test]
    [Category("Product-Bug")]
    public async Task GetEventById_WithVeryLargeId_Returns404()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEventByRawId("99999999999999999999");

        ApiAssert.Error(response, 404, "Event not found");
    }

    // Product bug: the spec declares id as integer, but a non-numeric id makes the API answer
    // 500 "Internal server error". Bad client input should be rejected with 400. Expected to fail until fixed.
    [TestCase("abc", TestName = "GetEventById_WithTextId_Returns400")]
    [TestCase("1abc", TestName = "GetEventById_WithNumberFollowedByTextId_Returns400")]
    [Category("Product-Bug")]
    public async Task GetEventById_WithNonNumericId_Returns400(string id)
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEventByRawId(id);

        ApiAssert.Error(response, 400, "Event not found");
    }

    // The spec lists no security or 401 for this endpoint, but the live API requires a token.
    [TestCase(null, "Unauthorized", TestName = "GetEventById_WithMissingToken_Returns401")]
    [TestCase("invalid.token.value", "Invalid or expired token", TestName = "GetEventById_WithInvalidToken_Returns401")]
    public async Task GetEventById_WithMissingOrInvalidToken_Returns401(string? token, string expectedError)
    {
        if (token is not null)
        {
            UseToken(token);
        }

        var response = await Clients.Events.GetEventById(1);

        ApiAssert.Error(response, 401, expectedError);
    }
}
