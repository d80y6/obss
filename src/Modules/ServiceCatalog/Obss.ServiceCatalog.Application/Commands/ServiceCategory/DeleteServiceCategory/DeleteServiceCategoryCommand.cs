using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.DeleteServiceCategory;

public sealed record DeleteServiceCategoryCommand(Guid Id) : IRequest;
