using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.UpdateNas;

public sealed record UpdateNasCommand(
    Guid Id,
    string Name,
    string NasIpAddress,
    string NasSecret,
    NasType NasType,
    string? Location) : IRequest<Result<NasDto>>;
