using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetReconciliations;

public sealed record GetReconciliationsQuery(string? Status, DateTime? FromDate, DateTime? ToDate, int Page = 1, int PageSize = 20) : IRequest<Result<IReadOnlyList<PaymentReconciliationDto>>>;
