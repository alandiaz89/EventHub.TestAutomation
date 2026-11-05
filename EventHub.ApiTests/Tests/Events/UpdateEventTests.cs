using Allure.NUnit;
using EventHub.ApiTests.Assertions;
using EventHub.ApiTests.Hooks;
using EventHub.Core.Builders;
using EventHub.Core.Models.Events;

namespace EventHub.ApiTests.Tests.Events;

[TestFixture]
[AllureNUnit]
[Category("API")]
[Category("Events")]
public class UpdateEventTests : ApiHooks
{
    [Test]
    public async Task UpdateEvent_ChangingEveryFieldExceptTitle_Returns200AndEchoesTheChanges()
    {
        var (created, changes) = await CreateEventAndPrepareChanges();

        var response = await Clients.Events.UpdateEvent(created.Id, changes);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.Multiple(() =>
        {
            Assert.That(response.Data?.Success, Is.True);
            Assert.That(response.Data?.Message, Is.EqualTo("Event updated successfully"));
            Assert.That(response.Data?.Data?.Id, Is.EqualTo(created.Id));
        });
        ApiAssert.EventMatches(changes, response.Data?.Data);
    }

    [Test]
    public async Task UpdateEvent_ChangingEveryFieldExceptTitle_PersistsTheChanges()
    {
        var (created, changes) = await CreateEventAndPrepareChanges();
        var updated = await Clients.Events.UpdateEvent(created.Id, changes);
        Assert.That((int)updated.StatusCode, Is.EqualTo(200), "Precondition: the update must succeed");

        var fetched = await Clients.Events.GetEventById(created.Id);

        Assert.That((int)fetched.StatusCode, Is.EqualTo(200));
        ApiAssert.EventMatches(changes, fetched.Data?.Data);
    }

    private async Task<(EventItem Created, CreateEventInput Changes)> CreateEventAndPrepareChanges()
    {
        await LoginAsConfiguredUser();
        var original = new CreateEventInputBuilder().Build();
        var created = (await Clients.Events.CreateEvent(original)).Data?.Data;
        TrackEvent(created);
        Assert.That(created, Is.Not.Null, "Precondition: the event to update must be created");

        var changes = new CreateEventInputBuilder()
            .WithTitle(original.Title)
            .WithDescription("Updated by API tests")
            .WithCategory(EventCategory.Workshop)
            .WithVenue("Updated Venue")
            .WithCity("Delhi")
            .WithEventDate(original.EventDate.AddDays(15))
            .WithPrice(250)
            .WithTotalSeats(20)
            .WithImageUrl("https://example.com/images/api-automation-event-updated.png")
            .Build();

        return (created!, changes);
    }
}
