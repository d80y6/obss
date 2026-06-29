using System.Text.Json;
using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateSegment;

public sealed class CreateSegmentCommandHandler : IRequestHandler<CreateSegmentCommand, Result<CustomerSegmentDto>>
{
    private readonly ICustomerSegmentRepository _segmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSegmentCommandHandler(ICustomerSegmentRepository segmentRepository, IUnitOfWork unitOfWork)
    {
        _segmentRepository = segmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerSegmentDto>> Handle(CreateSegmentCommand request, CancellationToken cancellationToken)
    {
        var criteria = JsonSerializer.Deserialize<SegmentCriteria>(request.Criteria)
            ?? new SegmentCriteria([]);

        var segment = CustomerSegment.Create(
            request.TenantId,
            request.Name,
            request.Description,
            criteria,
            request.Priority);

        await _segmentRepository.AddAsync(segment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = segment.Adapt<CustomerSegmentDto>() with { CustomerCount = 0 };
        return Result.Success(dto);
    }
}
