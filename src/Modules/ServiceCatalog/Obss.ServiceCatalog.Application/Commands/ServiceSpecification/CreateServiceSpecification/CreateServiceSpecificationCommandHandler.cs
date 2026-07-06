using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using DomainServiceSpecification = Obss.ServiceCatalog.Domain.Entities.ServiceSpecification;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.CreateServiceSpecification;

internal sealed class CreateServiceSpecificationCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<CreateServiceSpecificationCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceSpecificationCommand request, CancellationToken cancellationToken)
    {
        var spec = DomainServiceSpecification.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.Brand,
            request.Version,
            request.IsBundle,
            request.ValidFrom,
            request.ValidTo);

        await repository.AddAsync(spec, cancellationToken);
        return spec.Id;
    }
}
