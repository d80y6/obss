namespace Obss.CRM.Application.Contracts;

public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalCount);
