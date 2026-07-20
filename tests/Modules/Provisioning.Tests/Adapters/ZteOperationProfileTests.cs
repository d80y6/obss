using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.ZTE;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters;

public sealed class ZteOperationProfileTests
{
    [Fact]
    public void Default_profile_should_have_version_1_0_0()
    {
        var profile = ZteOperationProfile.Default;

        profile.ProfileVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void Default_profile_should_have_expected_confirmed_operations()
    {
        var profile = ZteOperationProfile.Default;

        profile.ConfirmedOperations.Should().Contain(ZteAdapterConstants.OperationActivateNumber);
        profile.ConfirmedOperations.Should().Contain(ZteAdapterConstants.OperationDeactivateNumber);
        profile.ConfirmedOperations.Should().Contain(ZteAdapterConstants.OperationCreateSubscriber);
        profile.ConfirmedOperations.Should().Contain(ZteAdapterConstants.OperationUpdateSubscriberProfile);
        profile.ConfirmedOperations.Should().Contain(ZteAdapterConstants.OperationSuspendSubscriber);
        profile.ConfirmedOperations.Should().Contain(ZteAdapterConstants.OperationResumeSubscriber);
        profile.ConfirmedOperations.Should().Contain(ZteAdapterConstants.OperationTestConnection);
    }

    [Fact]
    public void Default_profile_should_have_7_confirmed_operations()
    {
        var profile = ZteOperationProfile.Default;

        profile.ConfirmedOperations.Should().HaveCount(7);
    }

    [Fact]
    public void Default_profile_should_have_blocked_operations()
    {
        var profile = ZteOperationProfile.Default;

        profile.BlockedOperations.Should().Contain(ZteAdapterConstants.OperationSetCallForwarding);
        profile.BlockedOperations.Should().Contain(ZteAdapterConstants.OperationSetCallBarring);
        profile.BlockedOperations.Should().Contain(ZteAdapterConstants.OperationConfigurePriTrunk);
        profile.BlockedOperations.Should().Contain(ZteAdapterConstants.OperationIngestCallDataRecords);
    }

    [Fact]
    public void Confirmed_and_blocked_sets_should_not_overlap()
    {
        var profile = ZteOperationProfile.Default;

        var intersection = profile.ConfirmedOperations.Intersect(profile.BlockedOperations);
        intersection.Should().BeEmpty();
    }

    [Fact]
    public void IsOperationConfirmed_should_return_true_for_confirmed_operations()
    {
        var profile = ZteOperationProfile.Default;

        profile.IsOperationConfirmed(ZteAdapterConstants.OperationActivateNumber).Should().BeTrue();
        profile.IsOperationConfirmed(ZteAdapterConstants.OperationCreateSubscriber).Should().BeTrue();
    }

    [Fact]
    public void IsOperationConfirmed_should_return_false_for_blocked_operations()
    {
        var profile = ZteOperationProfile.Default;

        profile.IsOperationConfirmed(ZteAdapterConstants.OperationSetCallForwarding).Should().BeFalse();
        profile.IsOperationConfirmed(ZteAdapterConstants.OperationConfigurePriTrunk).Should().BeFalse();
    }

    [Fact]
    public void IsOperationBlocked_should_return_true_for_blocked_operations()
    {
        var profile = ZteOperationProfile.Default;

        profile.IsOperationBlocked(ZteAdapterConstants.OperationSetCallForwarding).Should().BeTrue();
        profile.IsOperationBlocked(ZteAdapterConstants.OperationConfigurePriTrunk).Should().BeTrue();
    }

    [Fact]
    public void IsOperationBlocked_should_return_false_for_confirmed_operations()
    {
        var profile = ZteOperationProfile.Default;

        profile.IsOperationBlocked(ZteAdapterConstants.OperationActivateNumber).Should().BeFalse();
        profile.IsOperationBlocked(ZteAdapterConstants.OperationCreateSubscriber).Should().BeFalse();
    }

    [Fact]
    public void Custom_profile_should_allow_override()
    {
        var profile = new ZteOperationProfile
        {
            ProfileVersion = "2.0.0",
            VendorProduct = "ZTE CX500",
            ConfirmedOperations = new HashSet<string> { "CustomOp" },
            BlockedOperations = new HashSet<string> { "BlockedOp" },
        };

        profile.ProfileVersion.Should().Be("2.0.0");
        profile.VendorProduct.Should().Be("ZTE CX500");
        profile.IsOperationConfirmed("CustomOp").Should().BeTrue();
        profile.IsOperationBlocked("BlockedOp").Should().BeTrue();
    }
}
