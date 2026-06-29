using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.CreatePaymentArrangement;

public sealed record CreatePaymentArrangementCommand(
    Guid CollectionCaseId,
    decimal TotalAmount,
    int InstallmentCount,
    decimal InstallmentAmount,
    string Frequency,
    DateTime FirstPaymentDate) : IRequest<Result<CollectionCaseDto>>;
