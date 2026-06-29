using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetCustomerSegments;

public sealed class GetCustomerSegmentsQueryHandler : IRequestHandler<GetCustomerSegmentsQuery, Result<IReadOnlyList<CustomerSegmentDto>>>
{
    private readonly ICustomerSegmentRepository _segmentRepository;

    public GetCustomerSegmentsQueryHandler(ICustomerSegmentRepository segmentRepository)
    {
        _segmentRepository = segmentRepository;
    }

    public async Task<Result<IReadOnlyList<CustomerSegmentDto>>> Handle(GetCustomerSegmentsQuery request, CancellationToken cancellationToken)
    {
        var segments = await _segmentRepository.GetAllAsync(cancellationToken);
        var dtos = new List<CustomerSegmentDto>();

        foreach (var segment in segments)
        {
            var customerCount = await _segmentRepository.GetCustomerCountAsync(segment.Id, cancellationToken);
            var dto = segment.Adapt<CustomerSegmentDto>() with { CustomerCount = customerCount };
            dtos.Add(dto);
        }

        return Result.Success<IReadOnlyList<CustomerSegmentDto>>(dtos);
    }
}
