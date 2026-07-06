using MediatR;
using Obss.NumberInventory.Application.DTOs;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Queries.SearchNumbers;

public sealed record SearchNumbersQuery(
    string? Prefix,
    NumberStatus? Status,
    NumberType? Type,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<TelephoneNumberDto>>>;
