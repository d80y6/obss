using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemoveCharacteristicValue;

public sealed record RemoveCharacteristicValueCommand(Guid ProductSpecificationId, Guid CharacteristicId, Guid ValueId) : IRequest<Result>;
