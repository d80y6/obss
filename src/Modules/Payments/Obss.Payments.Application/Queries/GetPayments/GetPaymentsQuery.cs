using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPayments;

public sealed record GetPaymentsQuery(
    Guid? CustomerId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<PaymentDto>>>;
