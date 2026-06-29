using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetRefunds;

public sealed record GetRefundsQuery(Guid? PaymentId, string? Status, DateTime? FromDate, DateTime? ToDate, int Page = 1, int PageSize = 20) : IRequest<Result<IReadOnlyList<RefundDto>>>;
