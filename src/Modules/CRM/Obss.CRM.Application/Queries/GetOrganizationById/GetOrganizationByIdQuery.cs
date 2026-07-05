using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(Guid Id) : IRequest<Result<OrganizationDto>>;
