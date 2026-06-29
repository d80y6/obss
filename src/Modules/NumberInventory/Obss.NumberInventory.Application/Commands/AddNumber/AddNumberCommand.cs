using MediatR;
using Obss.NumberInventory.Application.DTOs;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.AddNumber;

public sealed record AddNumberCommand(
    string Number,
    NumberType NumberType,
    decimal Cost,
    string Currency,
    string? Notes) : IRequest<Result<TelephoneNumberDto>>;
