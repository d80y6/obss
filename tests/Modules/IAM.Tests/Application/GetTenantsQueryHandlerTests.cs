using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.Queries.GetTenants;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Tests.Application;

public class GetTenantsQueryHandlerTests
{
    private readonly ITenantRepository _tenantRepository;
    private readonly GetTenantsQueryHandler _handler;

    public GetTenantsQueryHandlerTests()
    {
        _tenantRepository = Substitute.For<ITenantRepository>();
        _handler = new GetTenantsQueryHandler(_tenantRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTenants()
    {
        var query = new GetTenantsQuery();
        var tenants = new List<Tenant>
        {
            Tenant.Create("Tenant One", "tenant-one"),
            Tenant.Create("Tenant Two", "tenant-two"),
        };

        _tenantRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(tenants);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(t => t.Name == "Tenant One");
        result.Value.Should().Contain(t => t.Name == "Tenant Two");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoTenants()
    {
        var query = new GetTenantsQuery();

        _tenantRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tenant>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
