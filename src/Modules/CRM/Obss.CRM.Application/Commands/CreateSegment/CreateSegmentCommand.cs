using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateSegment;

public sealed record CreateSegmentCommand(
    string TenantId,
    string Name,
    string? Description,
    string Criteria,
    int Priority) : IRequest<Result<CustomerSegmentDto>>;
