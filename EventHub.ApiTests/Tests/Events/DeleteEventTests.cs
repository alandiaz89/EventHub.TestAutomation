using Allure.NUnit;
using EventHub.ApiTests.Assertions;
using EventHub.ApiTests.Hooks;
using EventHub.Core.Builders;
using EventHub.Core.Http;
using EventHub.Core.Services;
using EventHub.Core.Models.Common;

namespace EventHub.ApiTests.Tests.Events;

[TestFixture]
[AllureNUnit]
[Category("API")]
[Category("Events")]
public class DeleteEventTests : ApiHooks
{
    [Test]
    public async Task DeleteEvent_WithExistingEvent_Returns200AndEventIsGone()
    {
        await LoginAsConfiguredUser();
        var created = (await Services.Events.CreateEvent(new CreateEventRequestBuilder().Build())).Data?.Data;
        TrackEvent(created);
        Assert.That(created, Is.Not.Null, "Precondition: the event to delete must be created");

        var response = await Services.Events.DeleteEvent(created!.Id);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        var body = response.ReadAs<MessageResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(body?.Success, Is.True);
            Assert.That(body?.Message, Is.EqualTo("Event deleted successfully"));
        });

        var fetched = await Services.Events.GetEventById(created.Id);
        ApiAssert.Error(fetched, 404, $"Event with id {created.Id} not found");
    }

    [Test]
    public async Task DeleteEvent_WithNonExistentId_Returns404()
    {
        await LoginAsConfiguredUser();
        const int unknownId = 999999999;

        var response = await Services.Events.DeleteEvent(unknownId);

        ApiAssert.Error(response, 404, $"Event with id {unknownId} not found");
    }

    [Test]
    public async Task DeleteEvent_WithNonNumericId_Returns500()
    {
        await LoginAsConfiguredUser();

        var response = await Services.Events.DeleteEventByRawId("abc");

        ApiAssert.Error(response, 500, "Internal server error");
    }
}
