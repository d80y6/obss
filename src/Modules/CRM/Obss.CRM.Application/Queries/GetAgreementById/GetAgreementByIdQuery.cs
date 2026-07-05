using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetAgreementById;

public sealed record GetAgreementByIdQuery(Guid Id) : IRequest<Result<AgreementDto>>;
