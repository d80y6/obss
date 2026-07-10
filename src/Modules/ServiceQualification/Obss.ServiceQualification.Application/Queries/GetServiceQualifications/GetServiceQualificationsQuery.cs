using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.ServiceQualification.Application.DTOs;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Application.Queries.GetServiceQualifications;

public record GetServiceQualificationsQuery(
    Guid? CustomerId,
    DateTime? From,
    DateTime? To,
    QualificationResultType? Result) : IRequest<Result<List<ServiceQualificationDto>>>;
