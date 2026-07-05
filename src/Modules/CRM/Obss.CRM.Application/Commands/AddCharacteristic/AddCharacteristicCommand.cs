using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddCharacteristic;

public sealed record AddCharacteristicCommand(
    Guid CustomerId,
    string Key,
    string Value,
    string ValueType) : IRequest<Result>;
