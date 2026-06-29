using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.AssignCustomerToSegment;

public sealed class AssignCustomerToSegmentCommandHandler : IRequestHandler<AssignCustomerToSegmentCommand, Result>
{
    private readonly ICustomerSegmentRepository _segmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignCustomerToSegmentCommandHandler(ICustomerSegmentRepository segmentRepository, IUnitOfWork unitOfWork)
    {
        _segmentRepository = segmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AssignCustomerToSegmentCommand request, CancellationToken cancellationToken)
    {
        var segment = await _segmentRepository.GetByIdAsync(request.SegmentId, cancellationToken);

        if (segment is null)
            return Result.Failure(Error.NotFound(nameof(CustomerSegment), request.SegmentId));

        segment.AddCustomer(request.CustomerId, request.AssignedBy, false);

        await _segmentRepository.UpdateAsync(segment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
