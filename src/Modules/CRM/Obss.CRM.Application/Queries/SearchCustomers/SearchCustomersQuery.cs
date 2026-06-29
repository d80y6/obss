using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.SearchCustomers;

public sealed record SearchCustomersQuery(
    string? TenantId,
    string? Status,
    string? CustomerType,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<CustomerDto>>>;
