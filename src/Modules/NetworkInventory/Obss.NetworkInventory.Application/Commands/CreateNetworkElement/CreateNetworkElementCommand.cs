using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.CreateNetworkElement;

public sealed record CreateNetworkElementCommand(
    string TenantId,
    string Name,
    string Hostname,
    string IPAddress,
    string ElementType,
    string Vendor,
    string Model,
    string? SoftwareVersion,
    string? SerialNumber,
    string? Location,
    string? ManagementIP,
    string? SNMPCommunity,
    bool IsManaged) : IRequest<Result<NetworkElementDto>>;
