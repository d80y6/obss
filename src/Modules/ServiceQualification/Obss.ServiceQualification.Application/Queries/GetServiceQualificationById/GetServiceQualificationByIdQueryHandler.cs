using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Application.DTOs;

namespace Obss.ServiceQualification.Application.Queries.GetServiceQualificationById;

public class GetServiceQualificationByIdQueryHandler : IRequestHandler<GetServiceQualificationByIdQuery, Result<ServiceQualificationDto>>
{
    private readonly IServiceQualificationRepository _repository;

    public GetServiceQualificationByIdQueryHandler(IServiceQualificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ServiceQualificationDto>> Handle(
        GetServiceQualificationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var qualification = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (qualification is null)
            return Result.Failure<ServiceQualificationDto>(Error.NotFound("ServiceQualification", query.Id));

        var dto = qualification.Adapt<ServiceQualificationDto>();
        return Result.Success(dto);
    }
}
