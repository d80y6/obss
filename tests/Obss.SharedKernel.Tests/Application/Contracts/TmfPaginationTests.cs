using FluentAssertions;
using Obss.SharedKernel.Application.Contracts;
using Xunit;

namespace Obss.SharedKernel.Tests.Application.Contracts;

public class TmfPaginationTests
{
    [Fact]
    public void TmfPaginationRequest_Defaults_ShouldBeOffset0Limit20()
    {
        var request = new TmfPaginationRequest();
        request.Offset.Should().Be(0);
        request.Limit.Should().Be(20);
    }

    [Fact]
    public void TmfPaginationRequest_WhenLimitExceeds100_ShouldClampTo100()
    {
        var request = new TmfPaginationRequest { Limit = 500 };
        request.Limit.Should().Be(100);
    }

    [Fact]
    public void TmfPaginationResponse_SetPaginationHeaders_ShouldCalculateCorrectly()
    {
        var response = new TmfPaginationResponse();
        response.SetPaginationHeaders(0, 20, 100);

        response.Offset.Should().Be(0);
        response.Limit.Should().Be(20);
        response.TotalCount.Should().Be(100);
        response.ResultCount.Should().Be(20);
    }

    [Fact]
    public void TmfPaginationResponse_SetPaginationHeaders_OnLastPage_ShouldNotSetNextUrl()
    {
        var response = new TmfPaginationResponse();
        response.SetPaginationHeaders(80, 20, 100);

        response.ResultCount.Should().Be(20);
        response.NextUrl.Should().BeNull();
        response.PreviousUrl.Should().NotBeNull();
    }

    [Fact]
    public void TmfPaginationResponse_SetPaginationHeaders_OnFirstPage_ShouldNotSetPreviousUrl()
    {
        var response = new TmfPaginationResponse();
        response.SetPaginationHeaders(0, 20, 100);

        response.NextUrl.Should().NotBeNull();
        response.PreviousUrl.Should().BeNull();
    }
}
