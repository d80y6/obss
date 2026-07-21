using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Rating.Application.Abstractions;

public interface ICdrRecordRepository : IRepository<CdrRecord>
{
    Task<IReadOnlyList<CdrRecord>> GetByVendorAsync(string vendor, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CdrRecord>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    Task<CdrRecord?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
}
