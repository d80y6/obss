using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Transports.Restconf;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoAdapterConfig : AdapterConfigurationBase
{
    public string BaseUri { get; set; } = string.Empty;
    public bool UseSimulator { get; set; }
    public RestconfTransportConfig? RestconfTransport { get; set; }
}
