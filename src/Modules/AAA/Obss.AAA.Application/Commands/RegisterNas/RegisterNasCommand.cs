using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.RegisterNas;

public sealed record RegisterNasCommand(
    string Name,
    string NasIpAddress,
    string NasSecret,
    NasType NasType,
    string? Location) : IRequest<Result<NasDto>>;
