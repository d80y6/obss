using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.RecordArrangementPayment;

public sealed record RecordArrangementPaymentCommand(
    Guid PaymentArrangementId,
    decimal Amount) : IRequest<Result<CollectionCaseDto>>;
