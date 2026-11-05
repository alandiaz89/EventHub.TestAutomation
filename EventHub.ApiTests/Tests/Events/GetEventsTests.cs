using EventHub.ApiTests.Assertions;
using Allure.NUnit;
using EventHub.ApiTests.Hooks;
using EventHub.Core.Clients;
using EventHub.Core.Models.Common;
using EventHub.Core.Models.Events;

namespace EventHub.ApiTests.Tests.Events;

[TestFixture]
[AllureNUnit]
[Category("API")]
[Category("Events")]
public class GetEventsTests : ApiHooks
{
    [Test]
    public async Task GetEvents_WhenAuthenticated_Returns200()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEvents(new EventsInput {});

        System.Console.WriteLine(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Success, Is.True);
        });
    }

    [TestCase(null, "Unauthorized", TestName = "GetEvents_WithMissingToken_Returns401")]
    [TestCase("invalid.token.value", "Invalid or expired token", TestName = "GetEvents_WithInvalidToken_Returns401")]
    public async Task GetEvents_WithMissingOrInvalidToken_Returns401(string? token, string expectedError)
    {
        if (token is not null)
        {
            UseToken(token);
        }

        var response = await Clients.Events.GetEvents(new EventsInput());

        ApiAssert.Error(response, 401, expectedError);
    }

    // Product bug: the API currently answers 500 "Internal server error" for page=-1.
    // A client error (400) is the expected behaviour for an invalid page, so this test is expected to fail until fixed.
    [Test]
    [Category("Product-Bug")]
    public async Task GetEvents_WithNegativePage_Returns400()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEvents(new EventsInput { Page = -1 });

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }

    [TestCase(EventCategory.Conference, TestName = "GetEvents_WithCategoryConference_ReturnsOnlyConference")]
    [TestCase(EventCategory.Concert, TestName = "GetEvents_WithCategoryConcert_ReturnsOnlyConcert")]
    [TestCase(EventCategory.Festival, TestName = "GetEvents_WithCategoryFestival_ReturnsOnlyFestival")]
    public async Task GetEvents_WithCategoryThatHasEvents_ReturnsOnlyThatCategory(EventCategory category)
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEvents(new EventsInput { Category = category });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data?.Data, Is.Not.Empty);
        Assert.That(response.Data!.Data!.Select(e => e.Category), Is.All.EqualTo(category.ToString()));
    }

    [TestCase(EventCategory.Sports, TestName = "GetEvents_WithCategorySports_ReturnsEmptyList")]
    [TestCase(EventCategory.Workshop, TestName = "GetEvents_WithCategoryWorkshop_ReturnsEmptyList")]
    public async Task GetEvents_WithCategoryThatHasNoEvents_ReturnsEmptyList(EventCategory category)
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEvents(new EventsInput { Category = category });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Data, Is.Empty);
            Assert.That(response.Data.Pagination?.Total, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetEvents_WithLowercaseCategory_ReturnsSameEventsAsExactCase()
    {
        await LoginAsConfiguredUser();

        var exactCase = await Clients.Events.GetEvents(new EventsInput { Category = EventCategory.Festival });
        var lowerCase = await Clients.Events.GetEvents(new EventsInput { CategoryText = "festival" });

        Assert.That((int)exactCase.StatusCode, Is.EqualTo(200));
        Assert.That((int)lowerCase.StatusCode, Is.EqualTo(200));
        Assert.That(exactCase.Data?.Data, Is.Not.Empty);
        Assert.That(lowerCase.Data?.Data, Is.Not.Null);
        Assert.That(lowerCase.Data!.Data!.Select(e => e.Id), Is.EqualTo(exactCase.Data!.Data!.Select(e => e.Id)));
    }

    [Test]
    public async Task GetEvents_WithCityFilter_ReturnsOnlyEventsInThatCity()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEvents(new EventsInput { City = "Delhi" });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data?.Data, Is.Not.Empty);
        Assert.That(response.Data!.Data!.Select(e => e.City), Is.All.EqualTo("Delhi").IgnoreCase);
    }

    [Test]
    public async Task GetEvents_WithSearchTerm_ReturnsOnlyEventsMatchingTheTerm()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEvents(new EventsInput { Search = "Diwali" });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data?.Data, Is.Not.Empty);
        Assert.That(
            response.Data!.Data!.Select(e => $"{e.Title} {e.Description}"),
            Is.All.Contain("Diwali").IgnoreCase);
    }

    [TestCase("", "", TestName = "GetEvents_WithEmptyCityAndSearch_ReturnsAllEvents")]
    [TestCase("", null, TestName = "GetEvents_WithEmptyCity_ReturnsAllEvents")]
    [TestCase(null, "", TestName = "GetEvents_WithEmptySearch_ReturnsAllEvents")]
    public async Task GetEvents_WithEmptyTextFilters_ReturnsAllEvents(string? city, string? search)
    {
        await LoginAsConfiguredUser();
        var unfiltered = await Clients.Events.GetEvents(new EventsInput());

        var response = await Clients.Events.GetEvents(new EventsInput { City = city, Search = search });

        Assert.That((int)unfiltered.StatusCode, Is.EqualTo(200));
        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(unfiltered.Data?.Data, Is.Not.Empty);
        Assert.That(response.Data?.Data, Is.Not.Empty);

        // The list is shared and changes while tests run, so compare shape, not ids: empty filters must behave
        // like no filters (same page and limit, a full first page, a total in the same range).
        var plain = unfiltered.Data!.Pagination!;
        var empty = response.Data!.Pagination!;
        Assert.Multiple(() =>
        {
            Assert.That(empty.Page, Is.EqualTo(plain.Page));
            Assert.That(empty.Limit, Is.EqualTo(plain.Limit));
            Assert.That(response.Data.Data!.Count, Is.EqualTo(Math.Min(empty.Limit, empty.Total)));
            Assert.That(response.Data.Data.Count, Is.EqualTo(unfiltered.Data.Data!.Count));
            Assert.That(empty.Total, Is.EqualTo(plain.Total).Within(5).Percent);
        });
    }

    [Test]
    public async Task GetEvents_WithCategoryAndCity_ReturnsEventsMatchingBoth()
    {
        await LoginAsConfiguredUser();

        var response = await Clients.Events.GetEvents(new EventsInput { Category = EventCategory.Festival, City = "Delhi" });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Data?.Data, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data!.Data!.Select(e => e.Category), Is.All.EqualTo("Festival"));
            Assert.That(response.Data.Data!.Select(e => e.City), Is.All.EqualTo("Delhi").IgnoreCase);
        });
    }
}
