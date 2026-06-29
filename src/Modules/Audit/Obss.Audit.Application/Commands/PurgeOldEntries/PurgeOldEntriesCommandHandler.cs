using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.PurgeOldEntries;

public sealed class PurgeOldEntriesCommandHandler : IRequestHandler<PurgeOldEntriesCommand, Result<int>>
{
    private readonly IAuditEntryRepository _entryRepository;
    private readonly IRepository<AuditPolicy> _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PurgeOldEntriesCommandHandler> _logger;

    public PurgeOldEntriesCommandHandler(
        IAuditEntryRepository entryRepository,
        IRepository<AuditPolicy> policyRepository,
        IUnitOfWork unitOfWork,
        ILogger<PurgeOldEntriesCommandHandler> logger)
    {
        _entryRepository = entryRepository;
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(PurgeOldEntriesCommand request, CancellationToken cancellationToken)
    {
        var policies = await _policyRepository.GetAllAsync(cancellationToken);
        var activePolicies = policies
            .Where(p => p.IsActive)
            .GroupBy(p => p.EntityType)
            .ToDictionary(g => g.Key, g => g.Max(p => p.RetentionDays));

        if (activePolicies.Count == 0)
        {
            _logger.LogInformation("No active audit retention policies found. Skipping purge.");
            return Result.Success(0);
        }

        var deletedCount = await _entryRepository.DeleteExpiredEntriesAsync(activePolicies, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purged {Count} expired audit entries.", deletedCount);

        return Result.Success(deletedCount);
    }
}
