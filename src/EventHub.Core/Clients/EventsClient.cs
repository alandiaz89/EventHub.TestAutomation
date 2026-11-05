using EventHub.Core.Models.Events;
using RestSharp;

namespace EventHub.Core.Clients;

public sealed class EventsClient(ApiClient api)
{
    public Task<RestResponse<EventsResponse>> GetEvents(EventsInput input) =>
        api.Get<EventsResponse>("events", input.ToQuery());

    public Task<RestResponse<EventResponse>> CreateEvent(CreateEventInput input) =>
        api.Post<EventResponse>("events", input.ToPayload());

    /// <summary>Sends an arbitrary payload, for missing fields and wrong types.</summary>
    public Task<RestResponse<EventResponse>> CreateEvent(IDictionary<string, object?> payload) =>
        api.Post<EventResponse>("events", payload);

    /// <summary>Sends the text as the request body without serializing it, for malformed JSON.</summary>
    public Task<RestResponse<EventResponse>> CreateEventRaw(string jsonText) =>
        api.PostRawJson<EventResponse>("events", jsonText);

    public Task<RestResponse<EventResponse>> GetEventById(int id) =>
        api.Get<EventResponse>($"events/{id}");

    /// <summary>Requests <c>events/{id}</c> with any path segment, for non-numeric and out-of-range ids.</summary>
    public async Task<RestResponse> GetEventByRawId(string id) =>
        await api.Get<EventResponse>($"events/{id}");

    /// <summary>PUT replaces the whole event: every required field, including the title, must be sent.</summary>
    public Task<RestResponse<EventResponse>> UpdateEvent(int id, CreateEventInput input) =>
        api.Put<EventResponse>($"events/{id}", input.ToPayload());

    public Task<RestResponse> DeleteEvent(int id) =>
        api.Delete($"events/{id}");

    /// <summary>Requests DELETE <c>events/{id}</c> with any path segment, for non-numeric and out-of-range ids.</summary>
    public Task<RestResponse> DeleteEventByRawId(string id) =>
        api.Delete($"events/{id}");
}
