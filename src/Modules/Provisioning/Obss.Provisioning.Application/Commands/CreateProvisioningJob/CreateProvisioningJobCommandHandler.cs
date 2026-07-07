using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.IntegrationEvents;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningJob;

public sealed class CreateProvisioningJobCommandHandler : IRequestHandler<CreateProvisioningJobCommand, Result<ProvisioningJobDto>>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IProvisioningTemplateRepository _templateRepository;
    private readonly IOutboxService _outboxService;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProvisioningJobCommandHandler> _logger;

    public CreateProvisioningJobCommandHandler(
        IProvisioningJobRepository jobRepository,
        IProvisioningTemplateRepository templateRepository,
        IOutboxService outboxService,
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ILogger<CreateProvisioningJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _templateRepository = templateRepository;
        _outboxService = outboxService;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _logger = logger;
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
            await _jobRepository.AddAsync(job, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var integrationEvent = new WorkflowRequiredIntegrationEvent(
                job.Id,
                template.WorkflowDefinitionId,
                "ProvisioningJob",
                "system")
            {
                TenantId = request.TenantId.ToString(),
                CorrelationId = job.CorrelationId ?? string.Empty
            };

            await _outboxService.AddAsync(integrationEvent, cancellationToken);
            await _mediator.Publish(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Created provisioning job {JobId} with pending workflow for order {OrderId}",
                job.Id, request.OrderId);
        }
        else
        {
            job.Queue();
            await _jobRepository.AddAsync(job, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created provisioning job {JobId} for order {OrderId} (no workflow)",
                job.Id, request.OrderId);
        }

        return Result.Success(job.Adapt<ProvisioningJobDto>());
    }
}
