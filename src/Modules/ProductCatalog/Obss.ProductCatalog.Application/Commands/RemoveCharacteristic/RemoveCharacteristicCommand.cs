using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemoveCharacteristic;

public sealed record RemoveCharacteristicCommand(Guid ProductSpecificationId, Guid CharacteristicId) : IRequest<Result>;
