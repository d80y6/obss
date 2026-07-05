using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.PartialUpdateCustomer;

public sealed record PartialUpdateCustomerCommand(
    Guid Id,
    string? Description,
    string? StatusReason,
    string? ExternalId,
    DateTime? ValidFrom,
    DateTime? ValidUntil) : IRequest<Result<CustomerDto>>;
