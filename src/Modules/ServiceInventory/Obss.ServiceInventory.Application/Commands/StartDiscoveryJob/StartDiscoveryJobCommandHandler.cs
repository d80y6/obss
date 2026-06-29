using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.StartDiscoveryJob;

public sealed class StartDiscoveryJobCommandHandler : IRequestHandler<StartDiscoveryJobCommand, Result<DiscoveryJobDto>>
{
    private readonly IResourceDiscoveryJobRepository _discoveryJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartDiscoveryJobCommandHandler(
        IResourceDiscoveryJobRepository discoveryJobRepository,
        IUnitOfWork unitOfWork)
    {
        _discoveryJobRepository = discoveryJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DiscoveryJobDto>> Handle(StartDiscoveryJobCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DiscoveryType>(request.DiscoveryType, out var discoveryType))
            return Result.Failure<DiscoveryJobDto>(Error.Validation($"Invalid discovery type: '{request.DiscoveryType}'."));

        var job = ResourceDiscoveryJob.Create(
            request.TenantId,
            discoveryType,
            request.Configuration,
            request.CreatedBy);

        job.Start();

        await _discoveryJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(job.Adapt<DiscoveryJobDto>());
    }
}
