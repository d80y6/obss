using MediatR;
using Obss.AAA.Application.DTOs;

namespace Obss.AAA.Application.Queries.GetSessionsByUser;

public sealed record GetSessionsByUserQuery(string Username) : IRequest<IReadOnlyList<RadiusSessionDto>>;
