using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.UpdateNasStatus;

public sealed record UpdateNasStatusCommand(
    Guid NasId,
    string Status) : IRequest<Result<NasDto>>;
