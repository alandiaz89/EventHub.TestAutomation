using EventHub.Core.Models.Bookings;
using RestSharp;

namespace EventHub.Core.Clients;

public sealed class BookingsClient(ApiClient api)
{
    public Task<RestResponse<BookingsResponse>> GetBookings() =>
        api.Get<BookingsResponse>("bookings");

    /// <summary>Cancels the booking and gives its seats back to the event.</summary>
    public Task<RestResponse> CancelBooking(int id) =>
        api.Delete($"bookings/{id}");
}
