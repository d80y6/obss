using Xunit;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Obss.IAM.Application.Commands.CreateTenant;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Application.Queries.GetTenants;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Tests.Api;

public class TenantEndpointsTests
{
    private readonly IMediator _mediator;

    public TenantEndpointsTests()
    {
        _mediator = Substitute.For<IMediator>();
    }

    private static async Task<IResult> GetTenants(IMediator mediator)
    {
        var result = await mediator.Send(new GetTenantsQuery());
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> PostTenant(CreateTenantCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Created($"/api/v1/iam/tenants/{result.Value.Id}", result.Value)
            : Results.BadRequest(result.Error);
    }

    [Fact]
    public async Task GetTenants_ShouldReturnOk()
    {
        var tenants = new List<TenantDto>
        {
            new(Guid.NewGuid(), "Tenant One", "tenant-one", null, true, null, DateTime.UtcNow, DateTime.UtcNow),
        };
        _mediator.Send(Arg.Any<GetTenantsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<TenantDto>>(tenants));

        var response = await GetTenants(_mediator);

        response.Should().BeOfType<Ok<IReadOnlyList<TenantDto>>>();
        var ok = (Ok<IReadOnlyList<TenantDto>>)response;
        ok.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task PostTenant_WithValidCommand_ReturnsCreated()
    {
        var command = new CreateTenantCommand("New Tenant", "new-tenant", null, null, "admin", "admin@tenant.com", "Admin", "User");
        var tenantDto = new TenantDto(Guid.NewGuid(), "New Tenant", "new-tenant", null, true, null, DateTime.UtcNow, DateTime.UtcNow);
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(tenantDto));

        var response = await PostTenant(command, _mediator);

        response.Should().BeOfType<Created<TenantDto>>();
        var created = (Created<TenantDto>)response;
        created.Value.Should().Be(tenantDto);
        created.Location.Should().Contain(tenantDto.Id.ToString());
    }

    [Fact]
    public async Task PostTenant_WithInvalidCommand_ReturnsBadRequest()
    {
        var command = new CreateTenantCommand("", "", null, null, "admin", "admin@tenant.com", "Admin", "User");
        var error = Error.Validation("Name is required");
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<TenantDto>(error));

        var response = await PostTenant(command, _mediator);

        response.Should().BeOfType<BadRequest<Error>>();
        var badRequest = (BadRequest<Error>)response;
        badRequest.Value.Should().Be(error);
    }
}
