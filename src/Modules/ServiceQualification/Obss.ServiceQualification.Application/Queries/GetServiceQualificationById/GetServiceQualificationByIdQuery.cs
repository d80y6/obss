using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.ServiceQualification.Application.DTOs;

namespace Obss.ServiceQualification.Application.Queries.GetServiceQualificationById;

public record GetServiceQualificationByIdQuery(Guid Id) : IRequest<Result<ServiceQualificationDto>>;
