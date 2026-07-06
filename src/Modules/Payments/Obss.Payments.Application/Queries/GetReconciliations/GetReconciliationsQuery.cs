using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetReconciliations;

public sealed record GetReconciliationsQuery(string? Status, DateTime? FromDate, DateTime? ToDate, int Offset = 0, int Limit = 20) : IRequest<Result<IReadOnlyList<PaymentReconciliationDto>>>;
