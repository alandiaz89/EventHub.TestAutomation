namespace EventHub.Core.Models.Common;

public sealed record ErrorResponse(bool Success, string Error, IReadOnlyList<ErrorDetail>? Details = null);

public sealed record ErrorDetail(string Field, string Message);
