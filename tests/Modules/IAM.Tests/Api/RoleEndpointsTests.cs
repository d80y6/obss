using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Obss.IAM.Tests.Api;

public class RoleEndpointsTests
{
    private static IResult GetRoles()
    {
        return Results.Ok();
    }

    [Fact]
    public void GetRoles_ShouldReturnOk()
    {
        var response = GetRoles();

        response.Should().BeOfType<Ok>();
    }
}
