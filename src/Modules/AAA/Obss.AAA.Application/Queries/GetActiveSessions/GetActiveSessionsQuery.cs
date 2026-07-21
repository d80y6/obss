using MediatR;
using Obss.AAA.Application.DTOs;

namespace Obss.AAA.Application.Queries.GetActiveSessions;

public sealed record GetActiveSessionsQuery : IRequest<IReadOnlyList<RadiusSessionDto>>;
