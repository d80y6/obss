using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.UpdateNetworkElement;

public sealed record UpdateNetworkElementCommand(
    Guid Id,
    string Name,
    string Hostname,
    string Vendor,
    string Model,
    string? SoftwareVersion,
    string? SerialNumber,
    string? Location,
    string? ManagementIP,
    string? SNMPCommunity,
    bool IsManaged) : IRequest<Result<NetworkElementDto>>;
