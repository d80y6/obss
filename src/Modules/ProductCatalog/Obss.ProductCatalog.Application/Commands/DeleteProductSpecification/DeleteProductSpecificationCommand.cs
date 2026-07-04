using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.DeleteProductSpecification;

public sealed record DeleteProductSpecificationCommand(Guid ProductSpecificationId) : IRequest<Result>;
