using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.DeleteNas;

public sealed record DeleteNasCommand(Guid Id) : IRequest<Result<NasDto>>;
