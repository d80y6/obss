using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.CompleteDiscoveryJob;

public sealed class CompleteDiscoveryJobCommandHandler : IRequestHandler<CompleteDiscoveryJobCommand, Result<DiscoveryJobDto>>
{
    private readonly IResourceDiscoveryJobRepository _discoveryJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteDiscoveryJobCommandHandler(
        IResourceDiscoveryJobRepository discoveryJobRepository,
        IUnitOfWork unitOfWork)
    {
        _discoveryJobRepository = discoveryJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DiscoveryJobDto>> Handle(CompleteDiscoveryJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _discoveryJobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<DiscoveryJobDto>(Error.NotFound("ResourceDiscoveryJob", request.JobId));

        if (!string.IsNullOrEmpty(request.ErrorMessage))
        {
            job.Fail(request.ErrorMessage);
        }
        else
        {
            job.Complete(request.ResourcesFound, request.ResourcesMatched);
        }

        await _discoveryJobRepository.UpdateAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(job.Adapt<DiscoveryJobDto>());
    }
}
