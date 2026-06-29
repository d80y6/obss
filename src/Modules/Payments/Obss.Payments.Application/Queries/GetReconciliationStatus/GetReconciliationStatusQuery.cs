using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetReconciliationStatus;

public sealed record GetReconciliationStatusQuery(Guid ReconciliationId) : IRequest<Result<PaymentReconciliationDto>>;
