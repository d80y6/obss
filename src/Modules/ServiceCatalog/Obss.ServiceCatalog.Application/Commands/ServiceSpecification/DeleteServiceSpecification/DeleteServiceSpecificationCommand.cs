using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.DeleteServiceSpecification;

public sealed record DeleteServiceSpecificationCommand(Guid Id) : IRequest;
