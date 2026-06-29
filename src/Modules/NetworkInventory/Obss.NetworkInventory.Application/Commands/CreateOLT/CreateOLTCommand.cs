using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.CreateOLT;

public sealed record CreateOLTCommand(
    string TenantId,
    string Name,
    string Hostname,
    string IPAddress,
    string Vendor,
    string Model,
    string? SoftwareVersion,
    string? Location,
    int MaxPONPorts,
    int MaxONTPerPort,
    int MaxBandwidth) : IRequest<Result<OLTDetailDto>>;
