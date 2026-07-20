using FluentAssertions;
using Obss.Orders.Application.Services;
using Xunit;

namespace Obss.ServiceQualification.Tests;

public class FtthOrderDecompositionTests
{
    private readonly IFtthOrderDecompositionService _service;

    public FtthOrderDecompositionTests()
    {
        _service = new FtthOrderDecompositionService();
    }

    [Fact]
    public async Task DecomposeAsync_ForResidential_ReturnsCorrectTaskSequence()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            200,
            100,
            "SN-ONT-001",
            "LOID-001",
            "user@example.com",
            "123 Main St");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.CorrelationId.Should().NotBeEmpty();

        var resourceTasks = result.ResourceTasks;
        resourceTasks.Should().NotBeEmpty();
        resourceTasks[0].TaskType.Should().Be("ALLOCATE_PON_PORT");

        var serviceTasks = result.ServiceTasks;
        serviceTasks.Should().NotBeEmpty();
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_ONT_PROVISION");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_SERVICE_PORT_CONFIG");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_VLAN_CONFIG");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_PPPOE_CONFIG");
        serviceTasks.Should().Contain(t => t.TaskType == "PHYSICAL_INSTALL");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_ACTIVATION_TEST");
    }

    [Fact]
    public async Task DecomposeAsync_ForResidential_ExcludesBusinessTasks()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            200,
            100,
            "SN-ONT-001",
            "LOID-001",
            "user@example.com",
            "123 Main St");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.Should().NotContain(t => t.TaskType == "FTTH_SLA_CONFIG");
        result.ServiceTasks.Should().NotContain(t => t.TaskType == "FTTH_BACKUP_CONFIG");
        result.ResourceTasks.Should().NotContain(t => t.TaskType == "ALLOCATE_STATIC_IP");
    }

    [Fact]
    public async Task DecomposeAsync_ForBusiness_IncludesSlaAndBackupTasks()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "business",
            500,
            250,
            "SN-BIZ-002",
            "LOID-BIZ-001",
            "biz@company.com",
            "456 Corp Blvd");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.Should().Contain(t => t.TaskType == "FTTH_SLA_CONFIG");
        result.ServiceTasks.Should().Contain(t => t.TaskType == "FTTH_BACKUP_CONFIG");
        result.ResourceTasks.Should().Contain(t => t.TaskType == "ALLOCATE_STATIC_IP");
    }

    [Fact]
    public async Task DecomposeAsync_TaskOrdering_IsCorrect()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            100,
            50,
            "SN-003", null, null, "789 Pine St");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        var allTasks = result.ResourceTasks.Concat<object>(result.ServiceTasks).ToList();
        allTasks.Should().NotBeEmpty();

        var steps = allTasks.Select(t =>
        {
            return t is ResourceTask rt ? rt.StepOrder : ((ServiceTask)t).StepOrder;
        }).OrderBy(s => s).ToList();

        for (int i = 1; i < steps.Count; i++)
            steps[i].Should().BeGreaterThanOrEqualTo(steps[i - 1]);
    }

    [Fact]
    public async Task DecomposeAsync_TaskNames_IncludeArabic()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            100, 50, null, null, null, null);

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.All(t => !string.IsNullOrWhiteSpace(t.TaskNameAr)).Should().BeTrue();
        result.ServiceTasks.All(t => !string.IsNullOrWhiteSpace(t.TaskName)).Should().BeTrue();
    }
}
