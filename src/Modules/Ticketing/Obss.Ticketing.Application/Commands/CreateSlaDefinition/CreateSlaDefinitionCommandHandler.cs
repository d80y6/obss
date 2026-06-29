using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Commands.CreateSlaDefinition;

public sealed class CreateSlaDefinitionCommandHandler : IRequestHandler<CreateSlaDefinitionCommand, Result<SlaDefinitionDto>>
{
    private readonly ISlaDefinitionRepository _slaDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSlaDefinitionCommandHandler(
        ISlaDefinitionRepository slaDefinitionRepository,
        IUnitOfWork unitOfWork)
    {
        _slaDefinitionRepository = slaDefinitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SlaDefinitionDto>> Handle(CreateSlaDefinitionCommand request, CancellationToken cancellationToken)
    {
        var slaDefinition = SlaDefinition.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.Priority,
            request.ResponseTimeHours,
            request.ResolutionTimeHours,
            request.EscalationTimeHours);

        await _slaDefinitionRepository.AddAsync(slaDefinition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(slaDefinition.Adapt<SlaDefinitionDto>());
    }
}
