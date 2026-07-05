using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetIndividualById;

public sealed record GetIndividualByIdQuery(Guid Id) : IRequest<Result<IndividualDto>>;
