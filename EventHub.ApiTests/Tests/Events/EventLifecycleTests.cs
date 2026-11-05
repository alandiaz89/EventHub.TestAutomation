using Allure.Net.Commons;
using Allure.NUnit;
using EventHub.ApiTests.Assertions;
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
[Category("E2E")]
public class EventLifecycleTests : ApiHooks
{
    // One test on purpose: every step depends on the event the previous steps produced, so the flow stops at the
    // first failing step. Each step is a named Allure step with its own assertions, and ApiHooks prints every
    // request and response when the test fails.
    [Test]
    public async Task Event_CreateReadUpdateDelete_WorksOnTheSameEvent()
    {
        await LoginAsConfiguredUser();
        var original = new CreateEventInputBuilder().Build();
        var changes = new CreateEventInputBuilder()
            .WithTitle(original.Title)
            .WithDescription("Updated by the lifecycle test")
            .WithCategory(EventCategory.Workshop)
            .WithVenue("Lifecycle Venue")
            .WithCity("Delhi")
            .WithEventDate(original.EventDate.AddDays(10))
            .WithPrice(120)
            .WithTotalSeats(40)
            .WithImageUrl("https://example.com/images/lifecycle.png")
            .Build();

        var id = await AllureApi.Step("POST /events: create the event", async () =>
        {
            var response = await Clients.Events.CreateEvent(original);
            TrackEvent(response.Data?.Data);

            Assert.That((int)response.StatusCode, Is.EqualTo(201));
            Assert.That(response.Data?.Data?.Id, Is.GreaterThan(0));
            ApiAssert.EventMatches(original, response.Data?.Data);
            return response.Data!.Data!.Id;
        });

        await AllureApi.Step("GET /events/{id}: read the created event", async () =>
        {
            var response = await Clients.Events.GetEventById(id);

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            Assert.That(response.Data?.Data?.Id, Is.EqualTo(id));
            ApiAssert.EventMatches(original, response.Data?.Data);
        });

        await AllureApi.Step("PUT /events/{id}: update the event", async () =>
        {
            var response = await Clients.Events.UpdateEvent(id, changes);

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            Assert.That(response.Data?.Data?.Id, Is.EqualTo(id));
            ApiAssert.EventMatches(changes, response.Data?.Data);
        });

        await AllureApi.Step("GET /events/{id}: read the updated event", async () =>
        {
            var response = await Clients.Events.GetEventById(id);

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            ApiAssert.EventMatches(changes, response.Data?.Data);
        });

        await AllureApi.Step("DELETE /events/{id}: delete the event", async () =>
        {
            var response = await Clients.Events.DeleteEvent(id);

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            Assert.That(response.ReadAs<MessageResponse>()?.Success, Is.True);
        });

        await AllureApi.Step("GET /events/{id}: the deleted event is gone", async () =>
        {
            var response = await Clients.Events.GetEventById(id);

            ApiAssert.Error(response, 404, $"Event with id {id} not found");
        });
    }
}
