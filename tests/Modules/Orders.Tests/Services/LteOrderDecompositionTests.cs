using FluentAssertions;
using Obss.Orders.Application.Services;
using Xunit;

namespace Obss.Orders.Tests.Services;

public class LteOrderDecompositionTests
{
    private readonly LteOrderDecompositionService _service = new();

    [Fact]
    public async Task DecomposeAsync_ShouldReturnCorrelationId()
    {
        var request = CreateValidRequest();

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.CorrelationId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DecomposeAsync_ShouldCreateServiceAndResourceTasks()
    {
        var request = CreateValidRequest();

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.Should().NotBeEmpty();
        result.ResourceTasks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DecomposeAsync_ShouldStartWithSimActivation()
    {
        var request = CreateValidRequest();

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ResourceTasks[0].TaskType.Should().Be("LTE_SIM_ACTIVATE");
    }

    [Fact]
    public async Task DecomposeAsync_ShouldIncludeActivationTestAsLastStep()
    {
        var request = CreateValidRequest();

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        var maxStep = Math.Max(
            result.ServiceTasks.Max(t => t.StepOrder),
            result.ResourceTasks.Max(t => t.StepOrder));
        var lastService = result.ServiceTasks.FirstOrDefault(t => t.StepOrder == maxStep);
        var lastResource = result.ResourceTasks.FirstOrDefault(t => t.StepOrder == maxStep);
        (lastService?.TaskType ?? lastResource?.TaskType).Should().Be("LTE_ACTIVATION_TEST");
    }

    [Fact]
    public async Task DecomposeAsync_ShouldHaveArabicTaskNames()
    {
        var request = CreateValidRequest();

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        foreach (var task in result.ServiceTasks)
            task.TaskNameAr.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DecomposeAsync_ForBusinessSegment_ShouldIncludeStaticIp()
    {
        var request = CreateValidRequest() with { Segment = "business" };

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ResourceTasks.Should().Contain(t => t.TaskType == "LTE_STATIC_IP_ALLOCATE");
    }

    [Fact]
    public async Task DecomposeAsync_ForResidentialSegment_ShouldNotIncludeStaticIp()
    {
        var request = CreateValidRequest() with { Segment = "residential" };

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ResourceTasks.Should().NotContain(t => t.TaskType == "LTE_STATIC_IP_ALLOCATE");
    }

    private static LteDecompositionRequest CreateValidRequest()
    {
        return new LteDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            "8932012345678901234",
            "internet",
            20);
    }
}
