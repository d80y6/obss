using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.Commands.StartWorkflowInstance;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningJob;

public sealed class CreateProvisioningJobCommandHandler : IRequestHandler<CreateProvisioningJobCommand, Result<ProvisioningJobDto>>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IProvisioningTemplateRepository _templateRepository;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProvisioningJobCommandHandler(
        IProvisioningJobRepository jobRepository,
        IProvisioningTemplateRepository templateRepository,
        IMediator mediator,
        IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _templateRepository = templateRepository;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProvisioningJobDto>> Handle(CreateProvisioningJobCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ProvisioningAction>(request.Action, out var action))
            return Result.Failure<ProvisioningJobDto>(Error.Validation($"Invalid action: '{request.Action}'."));

        var job = ProvisioningJob.Create(
            request.TenantId,
            request.OrderId,
            request.OrderItemId,
            request.CustomerId,
            request.ServiceType,
            action);

        var template = await _templateRepository.GetByServiceTypeAndActionAsync(
            request.ServiceType, request.Action, cancellationToken);

        if (template is not null && template.IsActive)
        {
            var workflowResult = await _mediator.Send(
                new StartWorkflowInstanceCommand(
                    template.WorkflowDefinitionId,
                    "ProvisioningJob",
                    job.Id,
                    "system"),
                cancellationToken);

            if (workflowResult.IsSuccess)
            {
                job.AssignWorkflow(workflowResult.Value.Id);
            }
        }

        job.Queue();
        await _jobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(job.Adapt<ProvisioningJobDto>());
    }
}
