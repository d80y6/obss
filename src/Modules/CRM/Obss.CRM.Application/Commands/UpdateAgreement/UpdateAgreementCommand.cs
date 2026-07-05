using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateAgreement;

public sealed record UpdateAgreementCommand(
    Guid Id,
    string Name,
    string? Description) : IRequest<Result<AgreementDto>>;
