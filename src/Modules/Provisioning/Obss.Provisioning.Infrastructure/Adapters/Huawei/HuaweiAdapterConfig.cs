using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Huawei;

public sealed class HuaweiAdapterConfig : AdapterConfigurationBase
{
    public string? SnmpCommunity { get; set; }
    public int SnmpPort { get; set; } = 161;
    public string? DeviceModel { get; set; }
    public string? ControllerProfile { get; set; }
}
