using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetSlaDefinitions;

public sealed record GetSlaDefinitionsQuery(string? TenantId) : IRequest<Result<IReadOnlyList<SlaDefinitionDto>>>;
