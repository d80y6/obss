using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest<Result>;
