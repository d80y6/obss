using MediatR;
using Obss.ModuleTemplate.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ModuleTemplate.Application.Commands.CreateSample;

public sealed record CreateSampleCommand(
    string TenantId,
    string Name,
    string? Description) : IRequest<Result<SampleDto>>;
