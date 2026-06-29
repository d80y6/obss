using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.OpenCollectionCase;

public sealed class OpenCollectionCaseCommandHandler : IRequestHandler<OpenCollectionCaseCommand, Result<CollectionCaseDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<OpenCollectionCaseCommandHandler> _logger;

    public OpenCollectionCaseCommandHandler(
        ICollectionCaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<OpenCollectionCaseCommandHandler> logger)
    {
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<Result<CollectionCaseDto>> Handle(OpenCollectionCaseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;
        if (string.IsNullOrEmpty(tenantId))
            return Result.Failure<CollectionCaseDto>(Error.Unauthorized("Tenant context is required."));

        var existing = await _caseRepository.GetByCustomerWithActiveArrangementAsync(request.CustomerId, cancellationToken);
        if (existing is not null)
            return Result.Failure<CollectionCaseDto>(Error.Conflict($"An active collection case already exists for customer '{request.CustomerId}'."));

        var @case = CollectionCase.Open(
            tenantId,
            request.CustomerId,
            request.CustomerName,
            request.TotalOverdueAmount,
            request.Currency);

        await _caseRepository.AddAsync(@case, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Collection case {CaseId} opened for customer {CustomerId} with overdue amount {Amount} {Currency}.",
            @case.Id,
            request.CustomerId,
            request.TotalOverdueAmount,
            request.Currency);

        return Result.Success(@case.Adapt<CollectionCaseDto>());
    }
}
