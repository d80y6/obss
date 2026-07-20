using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Provisioning.Infrastructure.Adapters.ZTE;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters;

public sealed class ZteSimulatorTests
{
    private readonly ILogger<ZteSimulatorAdapter> _logger;
    private readonly ZteAdapterConfig _config;
    private readonly ZteOperationProfile _profile;
    private readonly IBlockedOperationStore _blockedStore;
    private readonly ZteSimulatorAdapter _sut;

    public ZteSimulatorTests()
    {
        _logger = Substitute.For<ILogger<ZteSimulatorAdapter>>();
        _config = new ZteAdapterConfig { EnableSimulator = true };
        _profile = ZteOperationProfile.Default;
        _blockedStore = new InMemoryBlockedOperationStore();
        _sut = new ZteSimulatorAdapter(_logger, _config, _profile, _blockedStore);
    }

    [Fact]
    public async Task ActivateNumber_with_valid_Yemen_number_should_succeed()
    {
        var request = new ActivateNumberRequest("+967712345678", "cust-001", "Mobile");

        var result = await _sut.ActivateNumberAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TelephoneNumber.Should().Be("+967712345678");
        result.Data.ActivationReference.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ActivateNumber_with_invalid_number_should_fail()
    {
        var request = new ActivateNumberRequest("+1111111111", null, null);

        var result = await _sut.ActivateNumberAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid Yemen telephone number");
    }

    [Fact]
    public async Task DeactivateNumber_should_succeed_for_active_number()
    {
        var activateRequest = new ActivateNumberRequest("+967712345678", "cust-001", "Mobile");
        await _sut.ActivateNumberAsync(activateRequest);

        var deactivateRequest = new DeactivateNumberRequest("+967712345678", "Customer request");
        var result = await _sut.DeactivateNumberAsync(deactivateRequest);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TelephoneNumber.Should().Be("+967712345678");
    }

    [Fact]
    public async Task CreateSubscriber_with_valid_data_should_succeed()
    {
        var request = new CreateSubscriberRequest(
            "+967712345678",
            "Ahmed Ali",
            "أحمد علي",
            "Sana'a, Yemen",
            "postpaid",
            "Premium",
            null);

        var result = await _sut.CreateSubscriberAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SubscriberId.Should().NotBeNullOrEmpty();
        result.Data.TelephoneNumber.Should().Be("+967712345678");
    }

    [Fact]
    public async Task CreateSubscriber_without_name_should_fail()
    {
        var request = new CreateSubscriberRequest(
            "+967712345678",
            null,
            null,
            null,
            null,
            null,
            null);

        var result = await _sut.CreateSubscriberAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("name");
    }

    [Fact]
    public async Task CreateSubscriber_with_invalid_number_should_fail()
    {
        var request = new CreateSubscriberRequest(
            "12345",
            "Test User",
            null,
            null,
            null,
            null,
            null);

        var result = await _sut.CreateSubscriberAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid Yemen telephone number");
    }

    [Fact]
    public async Task CreateSubscriber_duplicate_number_should_fail()
    {
        var request = new CreateSubscriberRequest(
            "+967712345678",
            "Ahmed Ali",
            null,
            null,
            null,
            null,
            null);
        await _sut.CreateSubscriberAsync(request);

        var duplicate = request with { CustomerName = "Another User" };
        var result = await _sut.CreateSubscriberAsync(duplicate);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateSubscriberProfile_should_update_existing_subscriber()
    {
        var createRequest = new CreateSubscriberRequest(
            "+967712345678",
            "Ahmed Ali",
            null,
            "Old Address",
            "postpaid",
            "Basic",
            null);
        var createResult = await _sut.CreateSubscriberAsync(createRequest);

        var updateRequest = new UpdateSubscriberProfileRequest(
            createResult.Data!.SubscriberId,
            "Ahmed Ali Updated",
            null,
            "New Address",
            "prepaid",
            "Premium",
            null);

        var result = await _sut.UpdateSubscriberProfileAsync(updateRequest);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateSubscriberProfile_for_nonexistent_subscriber_should_fail()
    {
        var request = new UpdateSubscriberProfileRequest(
            Guid.NewGuid().ToString("N"),
            "New Name",
            null,
            null,
            null,
            null,
            null);

        var result = await _sut.UpdateSubscriberProfileAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task SuspendSubscriber_should_suspend_active_subscriber()
    {
        var createRequest = new CreateSubscriberRequest(
            "+967712345678",
            "Ahmed Ali",
            null,
            null,
            null,
            null,
            null);
        var createResult = await _sut.CreateSubscriberAsync(createRequest);

        var result = await _sut.SuspendSubscriberAsync(
            new SuspendSubscriberRequest(createResult.Data!.SubscriberId, "Non-payment"));

        result.IsSuccess.Should().BeTrue();
        result.Data!.SubscriberId.Should().Be(createResult.Data.SubscriberId);
    }

    [Fact]
    public async Task ResumeSubscriber_should_resume_suspended_subscriber()
    {
        var createRequest = new CreateSubscriberRequest(
            "+967712345678",
            "Ahmed Ali",
            null,
            null,
            null,
            null,
            null);
        var createResult = await _sut.CreateSubscriberAsync(createRequest);
        await _sut.SuspendSubscriberAsync(
            new SuspendSubscriberRequest(createResult.Data!.SubscriberId, "Non-payment"));

        var result = await _sut.ResumeSubscriberAsync(
            new ResumeSubscriberRequest(createResult.Data.SubscriberId));

        result.IsSuccess.Should().BeTrue();
        result.Data!.SubscriberId.Should().Be(createResult.Data.SubscriberId);
    }

    [Fact]
    public async Task ResumeSubscriber_for_active_subscriber_should_fail()
    {
        var createRequest = new CreateSubscriberRequest(
            "+967712345678",
            "Ahmed Ali",
            null,
            null,
            null,
            null,
            null);
        var createResult = await _sut.CreateSubscriberAsync(createRequest);

        var result = await _sut.ResumeSubscriberAsync(
            new ResumeSubscriberRequest(createResult.Data!.SubscriberId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not suspended");
    }

    [Fact]
    public async Task TestConnection_should_succeed()
    {
        var result = await _sut.TestConnectionAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Connected.Should().BeTrue();
        result.Data.ServerVersion.Should().Be("ZTE-SIM/v1.0");
    }

    [Fact]
    public async Task Blocked_operations_should_return_Blocked_not_success()
    {
        var request = new SetCallForwardingRequest("sub-001", "unconditional", "+967711111111", null, null);

        var result = await _sut.SetCallForwardingAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
        result.BlockedReason.Should().Contain("not yet confirmed");
    }

    [Fact]
    public async Task Blocked_operations_should_persist_to_store()
    {
        var store = new InMemoryBlockedOperationStore();
        var adapter = new ZteSimulatorAdapter(_logger, _config, _profile, store);
        var request = new SetCallForwardingRequest("sub-001", "unconditional", "+967711111111", null, null);

        await adapter.SetCallForwardingAsync(request);

        store.SavedOperations.Should().NotBeEmpty();
        store.SavedOperations.Should().Contain(o => o.OperationName == ZteAdapterConstants.OperationSetCallForwarding);
    }

    [Fact]
    public async Task Failure_injection_should_cause_operations_to_fail()
    {
        _sut.FailureRate = 1.0;

        var request = new ActivateNumberRequest("+967712345678", "cust-001", "Mobile");
        var result = await _sut.ActivateNumberAsync(request);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Suspend_already_suspended_subscriber_should_fail()
    {
        var createRequest = new CreateSubscriberRequest(
            "+967712345678",
            "Ahmed Ali",
            null,
            null,
            null,
            null,
            null);
        var createResult = await _sut.CreateSubscriberAsync(createRequest);
        await _sut.SuspendSubscriberAsync(
            new SuspendSubscriberRequest(createResult.Data!.SubscriberId, "First suspension"));

        var result = await _sut.SuspendSubscriberAsync(
            new SuspendSubscriberRequest(createResult.Data.SubscriberId, "Second suspension"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already suspended");
    }

    [Fact]
    public async Task ClearState_should_reset_simulator()
    {
        await _sut.CreateSubscriberAsync(new CreateSubscriberRequest(
            "+967712345678", "Ahmed Ali", null, null, null, null, null));

        _sut.ClearState();

        _sut.SubscriberCount.Should().Be(0);
    }

    [Fact]
    public void AdapterName_should_include_simulator_suffix()
    {
        _sut.AdapterName.Should().Contain("Simulator");
    }

    private sealed class InMemoryBlockedOperationStore : IBlockedOperationStore
    {
        public List<BlockedOperation> SavedOperations { get; } = [];

        public Task SaveAsync(BlockedOperation operation, CancellationToken cancellationToken = default)
        {
            SavedOperations.Add(operation);
            return Task.CompletedTask;
        }
    }
}
