namespace EventHub.Core.Models.Common;

/// <summary>Body of endpoints that answer with only a status and a message, e.g. a successful delete.</summary>
public sealed record MessageResponse(bool Success, string? Message);
