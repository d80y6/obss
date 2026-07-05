using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateAgreement;

public sealed record CreateAgreementCommand(
    Guid CustomerId,
    string Name,
    string AgreementType) : IRequest<Result<AgreementDto>>;
