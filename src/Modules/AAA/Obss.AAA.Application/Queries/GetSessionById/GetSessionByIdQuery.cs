using MediatR;
using Obss.AAA.Application.DTOs;

namespace Obss.AAA.Application.Queries.GetSessionById;

public sealed record GetSessionByIdQuery(Guid Id) : IRequest<RadiusSessionDto?>;
