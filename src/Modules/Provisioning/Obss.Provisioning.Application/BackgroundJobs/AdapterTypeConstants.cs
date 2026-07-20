using System.Collections.ObjectModel;

namespace Obss.Provisioning.Application.BackgroundJobs;

public static class AdapterTypeConstants
{
    public const string HuaweiBroadband = "HUAWEI_BROADBAND";
    public const string ZteSoftswitch = "ZTE_SOFTSWITCH";
    public const string TestAdapter = "TEST_ADAPTER";
    public const string Simulated = "SIMULATED";

    public static readonly IReadOnlyDictionary<string, string> TechnologyMapping = new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>
        {
            ["FTTH"] = HuaweiBroadband,
            ["ADSL"] = HuaweiBroadband,
            ["LTE"] = HuaweiBroadband,
            ["WIFI"] = HuaweiBroadband,
            ["TELEPHONY"] = ZteSoftswitch,
            ["PRI"] = ZteSoftswitch,
            ["TDM"] = ZteSoftswitch,
            ["FREE_PHONE"] = ZteSoftswitch,
        });
}
