using System.Text.Json;
using Xunit;
using FluentAssertions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Persistence.Repositories;

namespace Obss.Provisioning.Tests;

public class RepositoryTests : ProvisioningIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveProvisioningJob()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningJobRepository(context);

        var tenantId = Guid.NewGuid();
        var job = ProvisioningJob.Create(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Internet",
            ProvisioningAction.Provision);

        await repository.AddAsync(job);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(job.Id);

        retrieved.Should().NotBeNull();
        retrieved!.TenantId.Should().Be(tenantId);
        retrieved.ServiceType.Should().Be("Internet");
        retrieved.Action.Should().Be(ProvisioningAction.Provision);
        retrieved.Status.Should().Be(JobStatus.Pending);
        retrieved.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task CanAddAndRetrieveProvisioningJobWithTasks()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningJobRepository(context);

        var job = ProvisioningJob.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VPN",
            ProvisioningAction.Activate);

        var task1 = ProvisioningTask.Create(job.Id, 1, ProvisioningTaskType.AccountSetup);
        var task2 = ProvisioningTask.Create(job.Id, 2, ProvisioningTaskType.NetworkConfig);
        job.AddTask(task1);
        job.AddTask(task2);

        await repository.AddAsync(job);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdWithTasksAsync(job.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Tasks.Should().HaveCount(2);
        retrieved.Tasks.Should().Contain(t => t.TaskType == ProvisioningTaskType.AccountSetup);
        retrieved.Tasks.Should().Contain(t => t.TaskType == ProvisioningTaskType.NetworkConfig);
    }

    [Fact]
    public async Task CanQueryQueuedJobs()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningJobRepository(context);

        var pendingJob = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Internet", ProvisioningAction.Provision);

        var queuedJob1 = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "VPN", ProvisioningAction.Activate);
        queuedJob1.Queue();

        var queuedJob2 = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "DNS", ProvisioningAction.Provision);
        queuedJob2.Queue();

        await repository.AddAsync(pendingJob);
        await repository.AddAsync(queuedJob1);
        await repository.AddAsync(queuedJob2);
        await context.SaveChangesAsync();

        var queuedJobs = await repository.GetQueuedJobsAsync();

        queuedJobs.Should().HaveCount(2);
        queuedJobs.Should().Contain(j => j.ServiceType == "VPN");
        queuedJobs.Should().Contain(j => j.ServiceType == "DNS");
        queuedJobs.Should().NotContain(j => j.ServiceType == "Internet");
    }

    [Fact]
    public async Task CanQueryFilteredJobs()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningJobRepository(context);

        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var job1 = ProvisioningJob.Create(
            tenantId, orderId, Guid.NewGuid(), Guid.NewGuid(),
            "Internet", ProvisioningAction.Provision);
        job1.Queue();

        var job2 = ProvisioningJob.Create(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "VPN", ProvisioningAction.Provision);
        job2.Queue();

        await repository.AddAsync(job1);
        await repository.AddAsync(job2);
        await context.SaveChangesAsync();

        var orderJobs = await repository.GetFilteredAsync(orderId, null, null, 1, 10);

        orderJobs.Should().HaveCount(1);
        orderJobs.Should().Contain(j => j.OrderId == orderId);

        var allQueued = await repository.GetFilteredAsync(null, "Queued", null, 1, 10);
        allQueued.Should().HaveCount(2);
    }

    [Fact]
    public async Task CanAddAndRetrieveProvisioningTemplate()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningTemplateRepository(context);

        var tenantId = Guid.NewGuid();
        var template = ProvisioningTemplate.Create(
            tenantId,
            "Internet Provisioning",
            "Standard internet provisioning workflow",
            "Internet",
            "Provision",
            Guid.NewGuid());

        await repository.AddAsync(template);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(template.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Internet Provisioning");
        retrieved.Description.Should().Be("Standard internet provisioning workflow");
        retrieved.ServiceType.Should().Be("Internet");
        retrieved.Action.Should().Be("Provision");
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanQueryTemplateByServiceTypeAndAction()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningTemplateRepository(context);

        var tenantId = Guid.NewGuid();

        var activateTemplate = ProvisioningTemplate.Create(
            tenantId, "VPN Activate", "VPN activation", "VPN", "Activate", Guid.NewGuid());

        var decommissionTemplate = ProvisioningTemplate.Create(
            tenantId, "VPN Decommission", "VPN decommission", "VPN", "Decommission", Guid.NewGuid());

        await repository.AddAsync(activateTemplate);
        await repository.AddAsync(decommissionTemplate);
        await context.SaveChangesAsync();

        var found = await repository.GetByServiceTypeAndActionAsync("VPN", "Activate");

        found.Should().NotBeNull();
        found!.Name.Should().Be("VPN Activate");
        found.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanDeactivateAndQueryTemplate()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningTemplateRepository(context);

        var template = ProvisioningTemplate.Create(
            Guid.NewGuid(), "Old Template", null, "Legacy", "Provision", Guid.NewGuid());
        template.Deactivate();

        await repository.AddAsync(template);
        await context.SaveChangesAsync();

        var notFound = await repository.GetByServiceTypeAndActionAsync("Legacy", "Provision");
        notFound.Should().BeNull();

        var deactivated = await repository.GetByIdAsync(template.Id);
        deactivated.Should().NotBeNull();
        deactivated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CanUpdateJobStatus()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningJobRepository(context);

        var job = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Internet", ProvisioningAction.Provision);

        await repository.AddAsync(job);
        await context.SaveChangesAsync();

        job.Queue();
        await context.SaveChangesAsync();

        var queued = await repository.GetByIdAsync(job.Id);
        queued!.Status.Should().Be(JobStatus.Queued);

        job.Start();
        await context.SaveChangesAsync();

        var started = await repository.GetByIdAsync(job.Id);
        started!.Status.Should().Be(JobStatus.InProgress);
        started.StartedAt.Should().NotBeNull();

        job.Complete();
        await context.SaveChangesAsync();

        var completed = await repository.GetByIdAsync(job.Id);
        completed!.Status.Should().Be(JobStatus.Completed);
        completed.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CanAssignServiceToJob()
    {
        using var context = CreateDbContext();
        var repository = new ProvisioningJobRepository(context);

        var job = ProvisioningJob.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Internet", ProvisioningAction.Provision);

        await repository.AddAsync(job);
        await context.SaveChangesAsync();

        var serviceId = Guid.NewGuid();
        job.AssignService(serviceId);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(job.Id);
        retrieved!.ServiceId.Should().Be(serviceId);
    }
}
