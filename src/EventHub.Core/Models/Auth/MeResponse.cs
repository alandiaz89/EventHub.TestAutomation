namespace EventHub.Core.Models.Auth;

public sealed record MeResponse(bool Success, MeUser? User);

public sealed record MeUser(int UserId, string Email, long Iat, long Exp);
