using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Transports.Netconf;
using Obss.Provisioning.Infrastructure.Transports.Rest;
using Obss.Provisioning.Infrastructure.Transports.Snmp;
using Obss.Provisioning.Infrastructure.Transports.Ssh;

namespace Obss.Provisioning.Infrastructure.Adapters.Huawei;

public sealed class HuaweiAdapterConfig : AdapterConfigurationBase
{
    public bool UseSimulator { get; set; }

    public SnmpTransportConfig? SnmpTransport { get; set; }
    public SshTransportConfig? SshTransport { get; set; }
    public NetconfTransportConfig? NetconfTransport { get; set; }
    public RestTransportConfig? RestTransport { get; set; }

    public SnmpTransportConfig? TryGetSnmpConfig()
        => UseSimulator ? null : SnmpTransport;

    public SshTransportConfig? TryGetSshConfig()
        => UseSimulator ? null : SshTransport;

    public NetconfTransportConfig? TryGetNetconfConfig()
        => UseSimulator ? null : NetconfTransport;

    public RestTransportConfig? TryGetRestConfig()
        => UseSimulator ? null : RestTransport;
}
