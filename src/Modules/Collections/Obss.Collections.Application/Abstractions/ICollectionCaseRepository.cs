using System.Linq.Expressions;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Collections.Application.Abstractions;

public interface ICollectionCaseRepository : IRepository<CollectionCase>
{
    Task<CollectionCase?> GetByIdWithDetailsAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CollectionCase>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CollectionCase>> GetByStatusAsync(CollectionCaseStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CollectionCase>> GetByDunningLevelAsync(int level, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CollectionCase>> GetActiveCasesAsync(CancellationToken cancellationToken = default);
    Task<CollectionCase?> GetByCustomerWithActiveArrangementAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, (int CaseCount, int CustomerCount, decimal TotalAmount)>> GetAgingBucketsAsync(string currency, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CollectionCase>> FindPagedAsync(Expression<Func<CollectionCase, bool>> predicate, int offset, int limit, CancellationToken cancellationToken = default);
}
