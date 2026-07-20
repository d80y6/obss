namespace Obss.Provisioning.Application.Abstractions;

public interface IAdapterRegistry
{
    void Register(string technologyType, string adapterName, IProvisioningAdapter adapter);

    IProvisioningAdapter? GetAdapter(string technologyType, string adapterName);

    IEnumerable<(string Technology, string Name, bool IsHealthy)> GetAllAdapters();
}
