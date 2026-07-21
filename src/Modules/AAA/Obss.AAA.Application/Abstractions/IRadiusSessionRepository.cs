using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Application.Abstractions;

public interface IRadiusSessionRepository : IRepository<RadiusSession>
{
    Task<RadiusSession?> GetBySessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RadiusSession>> GetActiveSessionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RadiusSession>> GetActiveSessionsByNasAsync(
        Guid nasId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RadiusSession>> GetSessionsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RadiusSession>> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RadiusSession>> GetByNasIdAsync(
        string nasId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RadiusSession>> GetSessionsByStatusAsync(
        SessionStatus status,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveSessionsAsync(
        CancellationToken cancellationToken = default);
}
