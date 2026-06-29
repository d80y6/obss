using Obss.NumberInventory.Domain.Entities;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NumberInventory.Application.Abstractions;

public interface ITelephoneNumberRepository : IRepository<TelephoneNumber>
{
    Task<IReadOnlyList<TelephoneNumber>> GetAvailableNumbersAsync(
        NumberType? numberType,
        string? prefix,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelephoneNumber>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<TelephoneNumber?> GetByNumberAsync(
        string number,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelephoneNumber>> SearchNumbersAsync(
        string? prefix,
        NumberStatus? status,
        NumberType? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
