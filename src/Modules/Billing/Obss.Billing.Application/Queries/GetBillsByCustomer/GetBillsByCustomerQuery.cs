using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillsByCustomer;

public sealed record GetBillsByCustomerQuery(
    Guid? CustomerId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<BillDto>>>;
