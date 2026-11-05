using System.Text.Json;
using EventHub.Core.Clients;
using EventHub.Core.Models.Common;
using EventHub.Core.Models.Events;
using RestSharp;

namespace EventHub.ApiTests.Assertions;

public static class ApiAssert
{
    public static void Error(RestResponse response, int expectedStatus, string expectedError)
    {
        Assert.That((int)response.StatusCode, Is.EqualTo(expectedStatus));
        var error = response.ReadAs<ErrorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(error?.Success, Is.False);
            Assert.That(error?.Error, Is.EqualTo(expectedError));
        });
    }

    public static void ValidationError(RestResponse response, string field, string message, int? expectedDetailCount = null)
    {
        Assert.That((int)response.StatusCode, Is.EqualTo(400));
        var error = response.ReadAs<ErrorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(error?.Success, Is.False);
            Assert.That(error?.Error, Is.EqualTo("Validation failed"));
            Assert.That(error?.Details, Has.Some.Matches<ErrorDetail>(d => d.Field == field && d.Message == message));
            if (expectedDetailCount is not null)
            {
                Assert.That(error?.Details, Has.Count.EqualTo(expectedDetailCount));
            }
        });
    }

    /// <summary>Checks every field the API echoes back for an event against the input that was sent.</summary>
    public static void EventMatches(CreateEventInput expected, EventItem? actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actual!.Title, Is.EqualTo(expected.Title));
            Assert.That(actual.Description, Is.EqualTo(expected.Description));
            Assert.That(actual.Category, Is.EqualTo(expected.Category));
            Assert.That(actual.Venue, Is.EqualTo(expected.Venue));
            Assert.That(actual.City, Is.EqualTo(expected.City));
            Assert.That(actual.EventDate, Is.EqualTo(expected.EventDate).Within(TimeSpan.FromSeconds(1)));
            // The API returns price as a string (see the possible-bug note in CreateEventTests).
            Assert.That(decimal.Parse(actual.Price!, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(expected.Price));
            Assert.That(actual.TotalSeats, Is.EqualTo(expected.TotalSeats));
            Assert.That(actual.ImageUrl, Is.EqualTo(expected.ImageUrl));
        });
    }

    public static void JsonFieldTypes(JsonElement element, JsonValueKind kind, IEnumerable<string> fields, bool nullable = false)
    {
        var expected = nullable ? $"{kind} or Null" : kind.ToString();
        Assert.Multiple(() =>
        {
            foreach (var field in fields)
            {
                if (!element.TryGetProperty(field, out var value))
                {
                    Assert.Fail($"Field '{field}' is missing from the response (expected {expected})");
                    continue;
                }

                var matches = value.ValueKind == kind || (nullable && value.ValueKind == JsonValueKind.Null);
                if (!matches)
                {
                    Assert.Fail($"Field '{field}' should be {expected} but was {value.ValueKind}");
                }
            }
        });
    }

    public static void ValidationFields(RestResponse response, params string[] fields)
    {
        Assert.That((int)response.StatusCode, Is.EqualTo(400));
        var error = response.ReadAs<ErrorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(error?.Success, Is.False);
            Assert.That(error?.Error, Is.EqualTo("Validation failed"));
            Assert.That(error?.Details?.Select(d => d.Field).Distinct(), Is.EquivalentTo(fields));
        });
    }
}
