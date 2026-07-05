using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveCharacteristic;

public sealed record RemoveCharacteristicCommand(Guid CustomerId, string Key) : IRequest<Result>;
