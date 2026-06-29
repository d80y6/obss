using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Queries.GetCustomerSegmentAssignments;

public sealed class GetCustomerSegmentAssignmentsQueryHandler : IRequestHandler<GetCustomerSegmentAssignmentsQuery, Result<IReadOnlyList<CustomerSegmentAssignmentDto>>>
{
    private readonly ICustomerSegmentRepository _segmentRepository;

    public GetCustomerSegmentAssignmentsQueryHandler(ICustomerSegmentRepository segmentRepository)
    {
        _segmentRepository = segmentRepository;
    }

    public async Task<Result<IReadOnlyList<CustomerSegmentAssignmentDto>>> Handle(GetCustomerSegmentAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var segment = await _segmentRepository.GetByIdAsync(request.SegmentId, cancellationToken);

        if (segment is null)
            return Result.Failure<IReadOnlyList<CustomerSegmentAssignmentDto>>(Error.NotFound(nameof(CustomerSegment), request.SegmentId));

        var assignments = await _segmentRepository.GetAssignmentsAsync(request.SegmentId, cancellationToken);
        var dtos = assignments.Adapt<List<CustomerSegmentAssignmentDto>>();

        return Result.Success<IReadOnlyList<CustomerSegmentAssignmentDto>>(dtos);
    }
}
