using MediatR;
using Obss.NumberInventory.Application.DTOs;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Queries.GetAvailableNumbers;

public sealed record GetAvailableNumbersQuery(
    NumberType? NumberType,
    string? Prefix,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<TelephoneNumberDto>>>;
