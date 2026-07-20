using FluentAssertions;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters;

public class AdapterRegistryTests
{
    private readonly AdapterRegistry _sut;

    public AdapterRegistryTests()
    {
        _sut = new AdapterRegistry();
    }

    [Fact]
    public void Register_ShouldStoreAdapterByTechnologyAndName()
    {
        var adapter = new StubAdapter("TestAdapter");

        _sut.Register("ftth", "TestAdapter", adapter);

        var retrieved = _sut.GetAdapter("ftth", "TestAdapter");
        retrieved.Should().BeSameAs(adapter);
    }

    [Fact]
    public void GetAdapter_WithUnknownTechnology_ShouldReturnNull()
    {
        var result = _sut.GetAdapter("nonexistent", "TestAdapter");
        result.Should().BeNull();
    }

    [Fact]
    public void GetAdapter_WithUnknownName_ShouldReturnNull()
    {
        var adapter = new StubAdapter("TestAdapter");
        _sut.Register("ftth", "TestAdapter", adapter);

        var result = _sut.GetAdapter("ftth", "WrongName");
        result.Should().BeNull();
    }

    [Fact]
    public void GetAdapter_ShouldBeCaseInsensitiveForTechnology()
    {
        var adapter = new StubAdapter("TestAdapter");
        _sut.Register("FTTH", "TestAdapter", adapter);

        var result = _sut.GetAdapter("ftth", "TestAdapter");
        result.Should().BeSameAs(adapter);
    }

    [Fact]
    public void Register_MultipleAdaptersForSameTechnology_ShouldResolveCorrectly()
    {
        var huawei = new StubAdapter("HuaweiBroadband");
        var zte = new StubAdapter("ZTEBroadband");
        _sut.Register("ftth", "HuaweiBroadband", huawei);
        _sut.Register("ftth", "ZTEBroadband", zte);

        var retrieved = _sut.GetAdapter("ftth", "ZTEBroadband");
        retrieved.Should().BeSameAs(zte);
    }

    [Fact]
    public void Register_SameNameInDifferentTechnologies_ShouldNotConflict()
    {
        var ftthAdapter = new StubAdapter("TestAdapter");
        var lteAdapter = new StubAdapter("TestAdapter");
        _sut.Register("ftth", "TestAdapter", ftthAdapter);
        _sut.Register("lte", "TestAdapter", lteAdapter);

        var ftthResult = _sut.GetAdapter("ftth", "TestAdapter");
        var lteResult = _sut.GetAdapter("lte", "TestAdapter");

        ftthResult.Should().BeSameAs(ftthAdapter);
        lteResult.Should().BeSameAs(lteAdapter);
    }

    [Fact]
    public void GetAllAdapters_WithNoRegistrations_ShouldReturnEmpty()
    {
        var all = _sut.GetAllAdapters();
        all.Should().BeEmpty();
    }

    [Fact]
    public void GetAllAdapters_ShouldReturnAllRegisteredAdapters()
    {
        var huawei = new StubAdapter("HuaweiBroadband");
        var zte = new StubAdapter("ZTEBroadband");
        _sut.Register("ftth", "HuaweiBroadband", huawei);
        _sut.Register("adsl", "ZTEBroadband", zte);

        var all = _sut.GetAllAdapters().ToList();

        all.Should().HaveCount(2);
        all.Should().Contain(a => a.Technology == "ftth" && a.Name == "HuaweiBroadband");
        all.Should().Contain(a => a.Technology == "adsl" && a.Name == "ZTEBroadband");
    }

    private sealed class StubAdapter : IProvisioningAdapter
    {
        public StubAdapter(string adapterName)
        {
            AdapterName = adapterName;
        }

        public string AdapterName { get; }

        public Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
        {
            return Task.FromResult(ProvisioningResult.Ok());
        }

        public Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
        {
            return Task.FromResult(ProvisioningResult.Ok());
        }
    }
}
