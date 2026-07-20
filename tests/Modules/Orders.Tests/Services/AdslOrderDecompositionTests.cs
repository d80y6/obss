using FluentAssertions;
using Obss.Orders.Application.Services;
using Xunit;

namespace Obss.Orders.Tests.Services;

public class AdslOrderDecompositionTests
{
    private readonly AdslOrderDecompositionService _service = new();

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
    public async Task DecomposeAsync_ShouldStartWithDslamPortAllocation()
    {
        var request = CreateValidRequest();

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.Should().Contain(t => t.TaskType == "ADSL_LINE_PROFILE_CONFIG");
        result.ResourceTasks.Should().Contain(t => t.TaskType == "ADSL_DSLAM_PORT_ALLOCATE");
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
        (lastService?.TaskType ?? lastResource?.TaskType).Should().Be("ADSL_ACTIVATION_TEST");
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
    public async Task DecomposeAsync_ShouldSetCorrectStepOrder()
    {
        var request = CreateValidRequest();

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        var serviceOrders = result.ServiceTasks.Select(t => t.StepOrder).ToList();
        var resourceOrders = result.ResourceTasks.Select(t => t.StepOrder).ToList();
        serviceOrders.Should().BeInAscendingOrder();
        resourceOrders.Should().BeInAscendingOrder();
    }

    private static AdslDecompositionRequest CreateValidRequest()
    {
        return new AdslDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            "DSLAM-01",
            0,
            35,
            "user@adsl",
            "ADSL_PROFILE_STANDARD");
    }
}
