using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateServiceSpecification;

internal sealed class UpdateServiceSpecificationCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<UpdateServiceSpecificationCommand>
{
    public async Task Handle(UpdateServiceSpecificationCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.Id} not found");

        spec.UpdateDetails(request.Name, request.Description, request.Brand, request.Version);
        spec.SetValidityPeriod(request.ValidFrom, request.ValidTo);
        await repository.UpdateAsync(spec, cancellationToken);
    }
}
