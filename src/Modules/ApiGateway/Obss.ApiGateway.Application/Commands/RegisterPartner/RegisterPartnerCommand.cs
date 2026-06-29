using MediatR;
using Obss.ApiGateway.Application.DTOs;
using Obss.ApiGateway.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.RegisterPartner;

public sealed record RegisterPartnerCommand(
    string Name,
    string ContactName,
    string ContactEmail,
    List<string>? AllowedIPs,
    SlaLevel SlaLevel,
    int MaxRequestsPerDay) : IRequest<Result<PartnerDto>>;
