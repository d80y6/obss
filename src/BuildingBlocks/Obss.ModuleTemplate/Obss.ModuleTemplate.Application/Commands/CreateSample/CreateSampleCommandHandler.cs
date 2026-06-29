using Mapster;
using MediatR;
using Obss.ModuleTemplate.Application.Abstractions;
using Obss.ModuleTemplate.Application.DTOs;
using Obss.ModuleTemplate.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ModuleTemplate.Application.Commands.CreateSample;

public sealed class CreateSampleCommandHandler : IRequestHandler<CreateSampleCommand, Result<SampleDto>>
{
    private readonly ISampleRepository _sampleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSampleCommandHandler(ISampleRepository sampleRepository, IUnitOfWork unitOfWork)
    {
        _sampleRepository = sampleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SampleDto>> Handle(CreateSampleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _sampleRepository.GetByNameAsync(request.Name, request.TenantId, cancellationToken);

        if (existing is not null)
            return Result.Failure<SampleDto>(Error.Conflict($"Sample with name '{request.Name}' already exists in this tenant."));

        var sample = SampleAggregate.Create(request.Name, request.TenantId, request.Description);

        await _sampleRepository.AddAsync(sample, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(sample.Adapt<SampleDto>());
    }
}
