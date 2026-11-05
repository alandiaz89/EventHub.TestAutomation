using System.Text.Json;
using EventHub.ApiTests.Assertions;
using Allure.NUnit;
using EventHub.ApiTests.Hooks;
using EventHub.Core.Builders;
using EventHub.Core.Clients;
using EventHub.Core.Models.Common;
using EventHub.Core.Models.Events;

namespace EventHub.ApiTests.Tests.Events;

[TestFixture]
[AllureNUnit]
[Category("API")]
[Category("Events")]
public class CreateEventTests : ApiHooks
{
    [Test]
    public async Task CreateEvent_WithValidData_Returns201AndEventIsRetrievable()
    {
        await LoginAsConfiguredUser();
        var input = new CreateEventInputBuilder().Build();

        var response = await Clients.Events.CreateEvent(input);
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        var created = response.Data?.Data;
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Success, Is.True);
            Assert.That(response.Data.Message, Is.EqualTo("Event created successfully"));
            Assert.That(created!.Id, Is.GreaterThan(0));
            Assert.That(created.Title, Is.EqualTo(input.Title));
            Assert.That(created.Category, Is.EqualTo(input.Category));
            Assert.That(created.City, Is.EqualTo(input.City));
            Assert.That(created.Venue, Is.EqualTo(input.Venue));
            Assert.That(created.TotalSeats, Is.EqualTo(input.TotalSeats));
            Assert.That(created.ImageUrl, Is.EqualTo(input.ImageUrl));
        });

        var fetched = await Clients.Events.GetEventById(created!.Id);

        Assert.That((int)fetched.StatusCode, Is.EqualTo(200));
        Assert.That(fetched.Data?.Data?.Title, Is.EqualTo(input.Title));
        Assert.That(fetched.Data?.Data?.ImageUrl, Is.EqualTo(input.ImageUrl));
    }

    // Possible product bug: the spec says price is a number, but the API returns it as a string ("100").]
    [Test]
    [Category("Product-Bug")]
    public async Task CreateEvent_WithValidData_ReturnsPriceAsJsonNumber()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(new CreateEventInputBuilder().WithPrice(100).Build());
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        using var json = JsonDocument.Parse(response.Content!);
        ApiAssert.JsonFieldTypes(json.RootElement.GetProperty("data"), JsonValueKind.Number, ["price"]);
    }

    // Possible product bug: posting the same title twice as the same user returns 201 both times with the
    // same event id and the second post silently overwrites the first. The spec documents no such upsert.
    [Test]
    [Category("Product-Bug")]
    public async Task CreateEvent_WithDuplicateTitle_DoesNotOverwriteExistingEvent()
    {
        await LoginAsConfiguredUser();
        var first = new CreateEventInputBuilder().WithDescription("first").Build();
        var second = new CreateEventInputBuilder().WithTitle(first.Title).WithDescription("second").Build();

        var firstResponse = await Clients.Events.CreateEvent(first);
        TrackEvent(firstResponse.Data?.Data);
        Assert.That((int)firstResponse.StatusCode, Is.EqualTo(201), "Arrange failed: the first event was not created");
        var secondResponse = await Clients.Events.CreateEvent(second);
        TrackEvent(secondResponse.Data?.Data);

        var fetched = await Clients.Events.GetEventById(firstResponse.Data!.Data!.Id);

        Assert.That(fetched.Data?.Data?.Description, Is.EqualTo("first"));
    }

    [Test]
    public async Task CreateEvent_WithoutImageUrl_Returns201AndImageUrlIsNull()
    {
        await LoginAsConfiguredUser();
        var input = new CreateEventInputBuilder().WithImageUrl(null).Build();

        var response = await Clients.Events.CreateEvent(input);
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        Assert.That(response.Data?.Data?.ImageUrl, Is.Null);
    }

    [Test]
    public async Task CreateEvent_WithValidData_SetsAvailableSeatsEqualToTotalSeats()
    {
        await LoginAsConfiguredUser();
        var input = new CreateEventInputBuilder().WithTotalSeats(25).Build();

        var response = await Clients.Events.CreateEvent(input);
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        Assert.Multiple(() =>
        {
            Assert.That(response.Data?.Data?.TotalSeats, Is.EqualTo(25));
            Assert.That(response.Data?.Data?.AvailableSeats, Is.EqualTo(25));
        });
    }

    [Test]
    public async Task CreateEvent_WithoutOptionalDescription_Returns201()
    {
        await LoginAsConfiguredUser();
        var payload = new CreateEventInputBuilder().Build().ToPayload();
        payload.Remove("description");

        var response = await Clients.Events.CreateEvent(payload);
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        Assert.That(response.Data?.Data?.Id, Is.GreaterThan(0));
    }

    [TestCase(1, TestName = "CreateEvent_WithOneSeat_Returns201")]
    [TestCase(10000, TestName = "CreateEvent_WithManySeats_Returns201")]
    public async Task CreateEvent_WithValidTotalSeats_Returns201(int totalSeats)
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(new CreateEventInputBuilder().WithTotalSeats(totalSeats).Build());
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        Assert.That(response.Data?.Data?.TotalSeats, Is.EqualTo(totalSeats));
    }

    [Test]
    public async Task CreateEvent_WithFreePrice_Returns201()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(new CreateEventInputBuilder().WithPrice(0).Build());
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        Assert.That(decimal.Parse(response.Data!.Data!.Price!, System.Globalization.CultureInfo.InvariantCulture), Is.Zero);
    }

    [TestCase(0, TestName = "CreateEvent_WithZeroSeats_Returns400")]
    [TestCase(-1, TestName = "CreateEvent_WithNegativeSeats_Returns400")]
    public async Task CreateEvent_WithNonPositiveTotalSeats_Returns400(int totalSeats)
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(new CreateEventInputBuilder().WithTotalSeats(totalSeats).Build());
        TrackEvent(response.Data?.Data);

        ApiAssert.ValidationError(response, "totalSeats", "Total seats must be a positive integer");
    }

    [Test]
    public async Task CreateEvent_WithNegativePrice_Returns400()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(new CreateEventInputBuilder().WithPrice(-1).Build());
        TrackEvent(response.Data?.Data);

        ApiAssert.ValidationError(response, "price", "Price must be a non-negative number");
    }

    [Test]
    public async Task CreateEvent_WithPastEventDate_Returns400()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(
            new CreateEventInputBuilder().WithEventDate(DateTimeOffset.UtcNow.AddDays(-1)).Build());
        TrackEvent(response.Data?.Data);

        ApiAssert.ValidationError(response, "eventDate", "Event date must be in the future");
    }

    [TestCase("title", "Title is required", TestName = "CreateEvent_WithoutTitle_Returns400")]
    [TestCase("category", "Category is required", TestName = "CreateEvent_WithoutCategory_Returns400")]
    [TestCase("venue", "Venue is required", TestName = "CreateEvent_WithoutVenue_Returns400")]
    [TestCase("city", "City is required", TestName = "CreateEvent_WithoutCity_Returns400")]
    [TestCase("eventDate", "Event date is required", TestName = "CreateEvent_WithoutEventDate_Returns400")]
    [TestCase("price", "Price is required", TestName = "CreateEvent_WithoutPrice_Returns400")]
    [TestCase("totalSeats", "Total seats is required", TestName = "CreateEvent_WithoutTotalSeats_Returns400")]
    public async Task CreateEvent_WithoutRequiredField_Returns400(string field, string expectedMessage)
    {
        await LoginAsConfiguredUser();
        var payload = new CreateEventInputBuilder().Build().ToPayload();
        payload.Remove(field);

        var response = await Clients.Events.CreateEvent(payload);
        TrackEvent(response.Data?.Data);

        ApiAssert.ValidationError(response, field, expectedMessage);
    }

    [TestCase("totalSeats", "abc", "Total seats must be a positive integer", TestName = "CreateEvent_WithTextTotalSeats_Returns400")]
    [TestCase("totalSeats", 2.5, "Total seats must be a positive integer", TestName = "CreateEvent_WithDecimalTotalSeats_Returns400")]
    [TestCase("price", "abc", "Price must be a non-negative number", TestName = "CreateEvent_WithTextPrice_Returns400")]
    [TestCase("eventDate", "not-a-date", "Event date must be a valid ISO 8601 date", TestName = "CreateEvent_WithInvalidEventDate_Returns400")]
    // The spec only says imageUrl is a string, but the live API validates it (observed message below).
    [TestCase("imageUrl", "not a url", "Image URL must be a valid URL", TestName = "CreateEvent_WithTextImageUrl_Returns400")]
    [TestCase("imageUrl", "http://", "Image URL must be a valid URL", TestName = "CreateEvent_WithImageUrlWithoutHost_Returns400")]
    [TestCase("imageUrl", "javascript:alert(1)", "Image URL must be a valid URL", TestName = "CreateEvent_WithJavascriptImageUrl_Returns400")]
    [TestCase("imageUrl", 123, "Image URL must be a valid URL", TestName = "CreateEvent_WithNumericImageUrl_Returns400")]
    public async Task CreateEvent_WithWrongFieldType_Returns400(string field, object value, string expectedMessage)
    {
        await LoginAsConfiguredUser();
        var payload = new CreateEventInputBuilder().Build().ToPayload();
        payload[field] = value;

        var response = await Clients.Events.CreateEvent(payload);
        TrackEvent(response.Data?.Data);

        ApiAssert.ValidationError(response, field, expectedMessage);
    }

    [Test]
    public async Task CreateEvent_WithEmptyBody_Returns400AndListsEveryRequiredField()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(new Dictionary<string, object?>());

        ApiAssert.ValidationFields(response, "title", "category", "venue", "city", "eventDate", "price", "totalSeats");
    }

    // Possible product bug: a title of 1000 characters makes the API answer 500 "Internal server error"
    // (200 characters already fails, 150 works). Invalid input should be rejected with 400 and a validation message.
    [Test]
    [Category("Product-Bug")]
    public async Task CreateEvent_WithVeryLongTitle_Returns400()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEvent(new CreateEventInputBuilder().WithTitle(new string('a', 1000)).Build());
        TrackEvent(response.Data?.Data);

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }

    // Possible product bug: malformed JSON makes the API answer 500 "Internal server error" instead of 400.
    [Test]
    [Category("Product-Bug")]
    public async Task CreateEvent_WithMalformedJson_Returns400()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.CreateEventRaw("{ \"title\": ");

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }

    [TestCase(null, "Unauthorized", TestName = "CreateEvent_WithMissingToken_Returns401")]
    [TestCase("invalid.token.value", "Invalid or expired token", TestName = "CreateEvent_WithInvalidToken_Returns401")]
    public async Task CreateEvent_WithMissingOrInvalidToken_Returns401(string? token, string expectedError)
    {
        if (token is not null)
        {
            UseToken(token);
        }

        var response = await Clients.Events.CreateEvent(new CreateEventInputBuilder().Build());
        TrackEvent(response.Data?.Data);

        ApiAssert.Error(response, 401, expectedError);
    }
}
