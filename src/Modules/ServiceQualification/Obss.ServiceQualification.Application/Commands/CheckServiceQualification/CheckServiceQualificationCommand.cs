using MediatR;
using Obss.ServiceQualification.Application.DTOs;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceQualification.Application.Commands.CheckServiceQualification;

public sealed record CheckServiceQualificationCommand(
    Guid CustomerId,
    GeographicAddressDto Address,
    List<QualificationRequestItem> RequestedServices,
    string? Description) : IRequest<Result<ServiceQualificationDto>>;
