using MediatR;
using Obss.CRM.Application.Contracts;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.SearchCustomers;

public sealed record SearchCustomersQuery(
    string? TenantId,
    string? Status,
    string? CustomerType,
    string? SearchTerm,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<CustomerDto>>>;
