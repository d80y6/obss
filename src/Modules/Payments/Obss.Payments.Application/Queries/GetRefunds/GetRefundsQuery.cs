using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetRefunds;

public sealed record GetRefundsQuery(Guid? PaymentId, string? Status, DateTime? FromDate, DateTime? ToDate, int Offset = 0, int Limit = 20) : IRequest<Result<IReadOnlyList<RefundDto>>>;
