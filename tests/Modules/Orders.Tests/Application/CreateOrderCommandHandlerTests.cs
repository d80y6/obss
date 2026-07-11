using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.CreateProductOrder;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Orders.Tests.Application;

public class CreateProductOrderCommandHandlerTests
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateProductOrderCommandHandler _handler;

    public CreateProductOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IProductOrderRepository>();
        _customerRepository = Substitute.For<ICustomerRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _offerRepository = Substitute.For<IOfferRepository>();
        _currentTenant = Substitute.For<ICurrentTenant>();
        _currentUser = Substitute.For<ICurrentUser>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CreateProductOrderCommandHandler(
            _orderRepository,
            _customerRepository,
            _productRepository,
            _offerRepository,
            _currentTenant,
            _currentUser,
            _unitOfWork);
    }

    private static CreateProductOrderCommand CreateValidCommand()
    {
        return new CreateProductOrderCommand(
            Guid.NewGuid(),
            "John Doe",
            "New",
            "Test order",
            "123 Main St", "New York", "NY", "10001", "US",
            null, null, null, null, null,
            "USD",
            null,
            null,
            null,
            null,
            new List<CreateProductOrderItemRequest>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Internet", "Basic Plan",
                    1, 49.99m, 29.99m, 5m, 8m, "Monthly")
            });
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenAllDataIsValid()
    {
        var command = CreateValidCommand();
        _currentTenant.TenantId.Returns("tenant-1");
        _currentUser.UserId.Returns("user-1");

        var customer = Customer.Create(
            "tenant-1", CustomerType.Residential, null, "John Doe",
            null, null, Email.Create("john@example.com"),
            null, null, "USD");
        _customerRepository.GetByIdAsync(command.CustomerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var product = Product.Create(
            "tenant-1", "Internet", null, null,
            ProductType.Service, false, true, null);
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(product);

        var offer = Offer.Create(
            "tenant-1", "Basic Plan", null,
            OfferType.Recurring, false, null, null, true, 0, null, null);
        _offerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(offer);

        _orderRepository.AddAsync(Arg.Any<ProductOrder>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ProductOrder>(null!));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CustomerName.Should().Be("John Doe");
        result.Value.Status.Should().Be("Draft");

        await _orderRepository.Received(1).AddAsync(Arg.Any<ProductOrder>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTenantIdIsMissing_ShouldReturnUnauthorized()
    {
        var command = CreateValidCommand();
        _currentTenant.TenantId.Returns((string?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldReturnNotFound()
    {
        var command = CreateValidCommand();
        _currentTenant.TenantId.Returns("tenant-1");
        _customerRepository.GetByIdAsync(command.CustomerId, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenOrderTypeIsInvalid_ShouldReturnValidationError()
    {
        var command = new CreateProductOrderCommand(
            Guid.NewGuid(), "John Doe", "InvalidType", null,
            null, null, null, null, null,
            null, null, null, null, null,
            "USD",
            null,
            null,
            null,
            null,
            new List<CreateProductOrderItemRequest>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Internet", "Basic",
                    1, 49.99m, 0, 0, 0, "Monthly")
            });
        _currentTenant.TenantId.Returns("tenant-1");
        _currentUser.UserId.Returns("user-1");
        _customerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Customer.Create("tenant-1", CustomerType.Residential, null, "John Doe",
                null, null, Email.Create("john@example.com"), null, null, "USD"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Description.Should().Contain("Invalid order type");
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldReturnNotFound()
    {
        var command = CreateValidCommand();
        _currentTenant.TenantId.Returns("tenant-1");
        _currentUser.UserId.Returns("user-1");
        _customerRepository.GetByIdAsync(command.CustomerId, Arg.Any<CancellationToken>())
            .Returns(Customer.Create("tenant-1", CustomerType.Residential, null, "John Doe",
                null, null, Email.Create("john@example.com"), null, null, "USD"));
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenOfferNotFound_ShouldReturnNotFound()
    {
        var command = CreateValidCommand();
        _currentTenant.TenantId.Returns("tenant-1");
        _currentUser.UserId.Returns("user-1");
        _customerRepository.GetByIdAsync(command.CustomerId, Arg.Any<CancellationToken>())
            .Returns(Customer.Create("tenant-1", CustomerType.Residential, null, "John Doe",
                null, null, Email.Create("john@example.com"), null, null, "USD"));
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Product.Create("tenant-1", "Internet", null, null,
                ProductType.Service, false, true, null));
        _offerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Offer?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenBillingPeriodIsInvalid_ShouldReturnValidationError()
    {
        var command = new CreateProductOrderCommand(
            Guid.NewGuid(), "John Doe", "New", null,
            null, null, null, null, null,
            null, null, null, null, null,
            "USD",
            null,
            null,
            null,
            null,
            new List<CreateProductOrderItemRequest>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Internet", "Basic",
                    1, 49.99m, 0, 0, 0, "InvalidPeriod")
            });
        _currentTenant.TenantId.Returns("tenant-1");
        _currentUser.UserId.Returns("user-1");
        _customerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Customer.Create("tenant-1", CustomerType.Residential, null, "John Doe",
                null, null, Email.Create("john@example.com"), null, null, "USD"));
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Product.Create("tenant-1", "Internet", null, null,
                ProductType.Service, false, true, null));
        _offerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Offer.Create("tenant-1", "Basic", null,
                OfferType.Recurring, false, null, null, true, 0, null, null));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Description.Should().Contain("Invalid billing period");
    }
}
