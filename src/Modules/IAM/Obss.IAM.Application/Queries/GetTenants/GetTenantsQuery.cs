using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetTenants;

public sealed record GetTenantsQuery() : IRequest<Result<IReadOnlyList<TenantDto>>>;
