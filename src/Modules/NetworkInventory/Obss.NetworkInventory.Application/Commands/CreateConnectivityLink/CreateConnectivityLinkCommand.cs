using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.CreateConnectivityLink;

public sealed record CreateConnectivityLinkCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid SourceElementId,
    Guid SourceInterfaceId,
    Guid TargetElementId,
    Guid TargetInterfaceId,
    string LinkType,
    int Bandwidth,
    string? Protocol,
    int LatencyMs,
    int MTU) : IRequest<Result<ConnectivityLinkDto>>;
