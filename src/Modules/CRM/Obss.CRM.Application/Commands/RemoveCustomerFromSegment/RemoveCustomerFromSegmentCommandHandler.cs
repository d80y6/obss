using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.RemoveCustomerFromSegment;

public sealed class RemoveCustomerFromSegmentCommandHandler : IRequestHandler<RemoveCustomerFromSegmentCommand, Result>
{
    private readonly ICustomerSegmentRepository _segmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCustomerFromSegmentCommandHandler(ICustomerSegmentRepository segmentRepository, IUnitOfWork unitOfWork)
    {
        _segmentRepository = segmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveCustomerFromSegmentCommand request, CancellationToken cancellationToken)
    {
        var segment = await _segmentRepository.GetByIdAsync(request.SegmentId, cancellationToken);

        if (segment is null)
            return Result.Failure(Error.NotFound(nameof(CustomerSegment), request.SegmentId));

        segment.RemoveCustomer(request.CustomerId);

        await _segmentRepository.UpdateAsync(segment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
