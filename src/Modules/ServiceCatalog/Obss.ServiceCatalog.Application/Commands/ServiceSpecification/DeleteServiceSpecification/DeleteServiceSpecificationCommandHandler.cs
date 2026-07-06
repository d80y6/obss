using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.DeleteServiceSpecification;

internal sealed class DeleteServiceSpecificationCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<DeleteServiceSpecificationCommand>
{
    public async Task Handle(DeleteServiceSpecificationCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.Id} not found");

        await repository.DeleteAsync(spec, cancellationToken);
    }
}
