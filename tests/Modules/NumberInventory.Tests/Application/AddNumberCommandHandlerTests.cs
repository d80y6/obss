using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.Commands.AddNumber;
using Obss.NumberInventory.Application.Mappings;
using Obss.NumberInventory.Domain.Entities;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Tests.Application;

public class AddNumberCommandHandlerTests
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly AddNumberCommandHandler _handler;

    public AddNumberCommandHandlerTests()
    {
        NumberInventoryMappingConfig.Configure();
        _numberRepository = Substitute.For<ITelephoneNumberRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentTenant = Substitute.For<ICurrentTenant>();
        _currentTenant.TenantId.Returns("test-tenant");
        _handler = new AddNumberCommandHandler(_numberRepository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_ShouldCreateNumber_WhenCommandIsValid()
    {
        var command = new AddNumberCommand("+1234567890", NumberType.Mobile, 10.0m, "USD", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Number.Should().Be("+1234567890");
        result.Value.NumberType.Should().Be("Mobile");
        result.Value.Status.Should().Be("Available");
        await _numberRepository.Received(1).AddAsync(Arg.Any<TelephoneNumber>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapAllPropertiesToDto()
    {
        var command = new AddNumberCommand("+1987654321", NumberType.Geographic, 5.50m, "EUR", "Office line");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("+1987654321");
        result.Value.NumberType.Should().Be("Geographic");
        result.Value.Status.Should().Be("Available");
        result.Value.Cost.Should().Be(5.50m);
        result.Value.Currency.Should().Be("EUR");
        result.Value.Notes.Should().Be("Office line");
        result.Value.TenantId.Should().Be("test-tenant");
        result.Value.CustomerId.Should().BeNull();
        result.Value.SubscriptionId.Should().BeNull();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _currentTenant.TenantId.Returns("custom-tenant");
        var command = new AddNumberCommand("+15551234567", NumberType.TollFree, 0m, "USD", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.TenantId.Should().Be("custom-tenant");
    }
}
