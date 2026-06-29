using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.Abstractions;

public interface IProvisioningTemplateRepository : IRepository<ProvisioningTemplate>
{
    Task<ProvisioningTemplate?> GetByServiceTypeAndActionAsync(
        string serviceType, string action, CancellationToken cancellationToken = default);
}
