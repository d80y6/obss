using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddAgreementRef;

public sealed record AddAgreementRefCommand(
    Guid CustomerId,
    Guid AgreementId,
    string Name,
    string AgreementType,
    string Role,
    string? Href) : IRequest<Result>;
