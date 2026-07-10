using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Application.DTOs;

namespace Obss.ServiceQualification.Application.Queries.GetServiceQualifications;

public class GetServiceQualificationsQueryHandler : IRequestHandler<GetServiceQualificationsQuery, Result<List<ServiceQualificationDto>>>
{
    private readonly IServiceQualificationRepository _repository;

    public GetServiceQualificationsQueryHandler(IServiceQualificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ServiceQualificationDto>>> Handle(
        GetServiceQualificationsQuery query,
        CancellationToken cancellationToken)
    {
        var qualifications = await _repository.GetAllAsync(cancellationToken);

        if (query.CustomerId.HasValue)
            qualifications = qualifications.Where(q => q.CustomerId == query.CustomerId.Value).ToList();

        if (query.From.HasValue)
            qualifications = qualifications.Where(q => q.RequestedDate >= query.From.Value).ToList();

        if (query.To.HasValue)
            qualifications = qualifications.Where(q => q.RequestedDate <= query.To.Value).ToList();

        var dtos = qualifications.Adapt<List<ServiceQualificationDto>>();
        return Result.Success(dtos);
    }
}
