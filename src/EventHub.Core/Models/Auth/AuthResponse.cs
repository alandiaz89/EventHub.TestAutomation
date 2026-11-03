namespace EventHub.Core.Models.Auth;

public sealed record AuthResponse(bool Success, string? Token, AuthUser? User);

public sealed record AuthUser(int Id, string Email);
