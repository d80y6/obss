using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetSegmentById;

public sealed class GetSegmentByIdQueryHandler : IRequestHandler<GetSegmentByIdQuery, Result<CustomerSegmentDto>>
{
    private readonly ICustomerSegmentRepository _segmentRepository;

    public GetSegmentByIdQueryHandler(ICustomerSegmentRepository segmentRepository)
    {
        _segmentRepository = segmentRepository;
    }

    public async Task<Result<CustomerSegmentDto>> Handle(GetSegmentByIdQuery request, CancellationToken cancellationToken)
    {
        var segment = await _segmentRepository.GetByIdAsync(request.SegmentId, cancellationToken);

        if (segment is null)
            return Result.Failure<CustomerSegmentDto>(Error.NotFound(nameof(CustomerSegment), request.SegmentId));

        var customerCount = await _segmentRepository.GetCustomerCountAsync(segment.Id, cancellationToken);
        var dto = segment.Adapt<CustomerSegmentDto>() with { CustomerCount = customerCount };

        return Result.Success(dto);
    }
}
