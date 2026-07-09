namespace Obss.Subscriptions.Domain.ValueObjects;

public sealed record Place(
    string? Id,
    string? Role,
    string? Name,
    string? Street,
    string? City,
    string? State,
    string? Zip,
    string? Country);
