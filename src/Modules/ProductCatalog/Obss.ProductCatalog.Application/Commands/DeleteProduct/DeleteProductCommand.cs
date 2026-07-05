using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : IRequest<Result>;
