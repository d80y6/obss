using System.Text.Json;
using FluentAssertions;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Application.Services;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters;

public class RouterDispatchTests
{
    [Fact]
    public void AdapterConstants_ShouldDefineRouterTechnologies()
    {
        AdapterConstants.TechnologyCiscoRouter.Should().Be("cisco_router");
        AdapterConstants.TechnologyJuniperRouter.Should().Be("juniper_router");
        AdapterConstants.TechnologyNokiaRouter.Should().Be("nokia_router");
    }

    [Fact]
    public void AdapterConstants_ShouldDefineRouterAdapterNames()
    {
        AdapterConstants.AdapterCiscoRouter.Should().Be("CISCO_ROUTER");
        AdapterConstants.AdapterJuniperRouter.Should().Be("JUNIPER_ROUTER");
        AdapterConstants.AdapterNokiaRouter.Should().Be("NOKIA_ROUTER");
    }

    [Fact]
    public void ProvisioningTaskType_ShouldHaveRouterValues()
    {
        ((int)ProvisioningTaskType.RouterInterfaceConfig).Should().Be(46);
        ((int)ProvisioningTaskType.RouterBgpConfig).Should().Be(47);
        ((int)ProvisioningTaskType.RouterOspfConfig).Should().Be(48);
        ((int)ProvisioningTaskType.RouterStaticRouteConfig).Should().Be(49);
        ((int)ProvisioningTaskType.RouterSystemConfig).Should().Be(50);
        ((int)ProvisioningTaskType.RouterAclConfig).Should().Be(51);
        ((int)ProvisioningTaskType.GetRouterStatus).Should().Be(52);
        ((int)ProvisioningTaskType.GetRouterInventory).Should().Be(53);
        ((int)ProvisioningTaskType.GetRouterAlarms).Should().Be(54);
    }

    [Fact]
    public void ResolveAdapterName_WithConfigVendorCisco_ShouldReturnCiscoAdapter()
    {
        var config = JsonDocument.Parse("{\"vendor\":\"cisco\"}");
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.RouterInterfaceConfig, null, config);
        name.Should().Be("CISCO_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_WithConfigVendorJuniper_ShouldReturnJuniperAdapter()
    {
        var config = JsonDocument.Parse("{\"vendor\":\"juniper\"}");
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.RouterBgpConfig, null, config);
        name.Should().Be("JUNIPER_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_WithConfigVendorNokia_ShouldReturnNokiaAdapter()
    {
        var config = JsonDocument.Parse("{\"vendor\":\"nokia\"}");
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.GetRouterStatus, null, config);
        name.Should().Be("NOKIA_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_WithAssignedToJuniper_ShouldResolveVendor()
    {
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.RouterInterfaceConfig, "juniper-router-01", null);
        name.Should().Be("JUNIPER_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_NonRouterTask_ShouldUseExistingMapping()
    {
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.FtthOntProvision, null, null);
        name.Should().Be("HUAWEI_BROADBAND");
    }

    [Fact]
    public void ResolveAdapterName_NoVendorHint_ShouldDefaultToCisco()
    {
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.RouterInterfaceConfig, null, null);
        name.Should().Be("CISCO_ROUTER");
    }
}
