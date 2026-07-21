using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetNasById;

public sealed record GetNasByIdQuery(Guid Id) : IRequest<Result<NasDto>>;
