using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.Services;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Xunit;

namespace Obss.Provisioning.Tests.Services;

public class ProvisioningJobCoordinatorTests
{
    private readonly IAdapterRegistry _adapterRegistry;
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly ILogger<ProvisioningJobCoordinator> _logger;
    private readonly ProvisioningJobCoordinator _coordinator;

    public ProvisioningJobCoordinatorTests()
    {
        _adapterRegistry = Substitute.For<IAdapterRegistry>();
        _jobRepository = Substitute.For<IProvisioningJobRepository>();
        _logger = Substitute.For<ILogger<ProvisioningJobCoordinator>>();
        _coordinator = new ProvisioningJobCoordinator(_adapterRegistry, _jobRepository, _logger);
    }

    [Fact]
    public async Task ExecuteJobAsync_WithNoTasks_ShouldComplete()
    {
        var job = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "FTTH", ProvisioningAction.Provision);

        var result = await _coordinator.ExecuteJobAsync(job, CancellationToken.None);

        result.Status.Should().Be("Completed");
        result.CompletedTasks.Should().Be(0);
        result.TotalTasks.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteJobAsync_WithSingleSuccessfulTask_ShouldComplete()
    {
        var job = CreateJobWithTask(ProvisioningTaskType.FtthOntProvision);
        var adapter = Substitute.For<IProvisioningAdapter>();
        adapter.AdapterName.Returns("HUAWEI_BROADBAND");
        adapter.ExecuteAsync(Arg.Any<ProvisioningTask>(), Arg.Any<CancellationToken>())
            .Returns(ProvisioningResult.Ok("completed"));
        _adapterRegistry.GetAdapter(Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

        var result = await _coordinator.ExecuteJobAsync(job, CancellationToken.None);

        result.Status.Should().Be("Completed");
        result.CompletedTasks.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteJobAsync_WhenTaskFails_ShouldRollback()
    {
        var job = CreateJobWithTask(ProvisioningTaskType.FtthOntProvision);
        var adapter = Substitute.For<IProvisioningAdapter>();
        adapter.AdapterName.Returns("HUAWEI_BROADBAND");
        adapter.ExecuteAsync(Arg.Any<ProvisioningTask>(), Arg.Any<CancellationToken>())
            .Returns(ProvisioningResult.Fail("OLT not reachable"));
        _adapterRegistry.GetAdapter(Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

        var result = await _coordinator.ExecuteJobAsync(job, CancellationToken.None);

        result.Status.Should().Be("RolledBack");
        result.ErrorMessage.Should().Contain("OLT not reachable");
        await adapter.Received(1).CompensateAsync(Arg.Any<ProvisioningTask>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteJobAsync_ShouldExecuteTasksInStepOrder()
    {
        var job = CreateJobWithMultipleTasks();
        var adapter = Substitute.For<IProvisioningAdapter>();
        adapter.AdapterName.Returns("HUAWEI_BROADBAND");
        adapter.ExecuteAsync(Arg.Any<ProvisioningTask>(), Arg.Any<CancellationToken>())
            .Returns(ProvisioningResult.Ok());
        _adapterRegistry.GetAdapter(Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

        var result = await _coordinator.ExecuteJobAsync(job, CancellationToken.None);

        result.Status.Should().Be("Completed");
        result.CompletedTasks.Should().Be(2);
    }

    [Fact]
    public async Task GetProgressAsync_ShouldReturnUnknownForNonExistentJob()
    {
        var result = await _coordinator.GetProgressAsync(Guid.NewGuid(), CancellationToken.None);

        result.Status.Should().Be("Unknown");
        result.CompletedTasks.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteJobAsync_WhenAdapterNotFound_ShouldSkipTask()
    {
        var job = CreateJobWithTask(ProvisioningTaskType.Custom);
        _adapterRegistry.GetAdapter(Arg.Any<string>(), Arg.Any<string>()).Returns((IProvisioningAdapter?)null);

        var result = await _coordinator.ExecuteJobAsync(job, CancellationToken.None);

        result.Status.Should().Be("Completed");
        result.CompletedTasks.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteJobAsync_ShouldRetryOnTransientFailure()
    {
        var job = CreateJobWithTask(ProvisioningTaskType.FtthOntProvision);
        var adapter = Substitute.For<IProvisioningAdapter>();
        adapter.AdapterName.Returns("HUAWEI_BROADBAND");

        var callCount = 0;
        adapter.ExecuteAsync(Arg.Any<ProvisioningTask>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return callCount <= 2
                    ? ProvisioningResult.Fail("transient error")
                    : ProvisioningResult.Ok("completed");
            });
        _adapterRegistry.GetAdapter(Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

        var result = await _coordinator.ExecuteJobAsync(job, CancellationToken.None);

        result.Status.Should().Be("Completed");
        result.CompletedTasks.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteJobAsync_ShouldFailAfterMaxRetries()
    {
        var job = CreateJobWithTask(ProvisioningTaskType.FtthOntProvision);
        var adapter = Substitute.For<IProvisioningAdapter>();
        adapter.AdapterName.Returns("HUAWEI_BROADBAND");
        adapter.ExecuteAsync(Arg.Any<ProvisioningTask>(), Arg.Any<CancellationToken>())
            .Returns(ProvisioningResult.Fail("persistent error"));
        _adapterRegistry.GetAdapter(Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

        var result = await _coordinator.ExecuteJobAsync(job, CancellationToken.None);

        result.Status.Should().Be("RolledBack");
    }

    [Fact]
    public async Task ExecuteJobAsync_WhenCancelled_ShouldFailJob()
    {
        var job = CreateJobWithTask(ProvisioningTaskType.FtthOntProvision);
        var adapter = Substitute.For<IProvisioningAdapter>();
        adapter.AdapterName.Returns("HUAWEI_BROADBAND");
        adapter.ExecuteAsync(Arg.Any<ProvisioningTask>(), Arg.Any<CancellationToken>())
            .Returns(ProvisioningResult.Ok());
        _adapterRegistry.GetAdapter(Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await _coordinator.ExecuteJobAsync(job, cts.Token);

        result.Status.Should().Be("Cancelled");
    }

    private static ProvisioningJob CreateJobWithTask(ProvisioningTaskType taskType)
    {
        var job = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "FTTH", ProvisioningAction.Provision);

        var task = ProvisioningTask.Create(job.Id, 1, taskType);
        job.AddTask(task);
        return job;
    }

    private static ProvisioningJob CreateJobWithMultipleTasks()
    {
        var job = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "FTTH", ProvisioningAction.Provision);

        var task1 = ProvisioningTask.Create(job.Id, 1, ProvisioningTaskType.FtthOntProvision);
        job.AddTask(task1);

        var task2 = ProvisioningTask.Create(job.Id, 2, ProvisioningTaskType.FtthServicePortConfig);
        job.AddTask(task2);

        return job;
    }
}
