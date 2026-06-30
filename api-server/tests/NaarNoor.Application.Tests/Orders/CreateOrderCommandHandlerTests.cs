using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Application.Orders.Commands.CreateOrder;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Application.Tests.Orders;

public class CreateOrderCommandHandlerTests : IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private CreateOrderCommandHandler _handler = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("CreateOrder_" + Guid.NewGuid())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _unitOfWork = new UnitOfWork(_dbContext);
        _handler = new CreateOrderCommandHandler(_unitOfWork);
    }

    public async Task DisposeAsync() => await _dbContext.DisposeAsync();

    private CreateOrderCommand BuildCommand(string type = "collection") =>
        new CreateOrderCommand(
            CustomerName: "Test Customer",
            Email: "test@example.com",
            PhoneNumber: "07700000000",
            Type: type,
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Momos", 8.95m, 2),
                new(Guid.NewGuid(), "Sel Roti", 4.50m, 1)
            },
            Notes: null,
            DeliveryAddress: null,
            TableReservationName: null
        );

    [Fact]
    public async Task Handle_CollectionOrder_ReturnsNewOrderId()
    {
        var result = await _handler.Handle(BuildCommand("collection"), CancellationToken.None);

        result.Should().NotBe(Guid.Empty, "Handler should return a valid Order ID");
    }

    [Fact]
    public async Task Handle_CollectionOrder_PersistsOrderToDatabase()
    {
        var orderId = await _handler.Handle(BuildCommand("collection"), CancellationToken.None);

        var order = await _dbContext.Orders.FindAsync(orderId);
        order.Should().NotBeNull();
        order!.CustomerName.Should().Be("Test Customer");
        order.Email.Should().Be("test@example.com");
        order.Type.Should().Be(OrderType.Collection);
    }

    [Fact]
    public async Task Handle_DeliveryType_SetsOrderTypeDelivery()
    {
        var orderId = await _handler.Handle(BuildCommand("delivery"), CancellationToken.None);

        var order = await _dbContext.Orders.FindAsync(orderId);
        order!.Type.Should().Be(OrderType.Delivery);
    }

    [Fact]
    public async Task Handle_DineInType_SetsOrderTypeDineIn()
    {
        var orderId = await _handler.Handle(BuildCommand("dine-in"), CancellationToken.None);

        var order = await _dbContext.Orders.FindAsync(orderId);
        order!.Type.Should().Be(OrderType.DineIn);
    }

    [Fact]
    public async Task Handle_UnknownType_DefaultsToCollection()
    {
        var orderId = await _handler.Handle(BuildCommand("takeaway"), CancellationToken.None);

        var order = await _dbContext.Orders.FindAsync(orderId);
        order!.Type.Should().Be(OrderType.Collection);
    }

    [Fact]
    public async Task Handle_CalculatesTotalAmount_FromItems()
    {
        var cmd = new CreateOrderCommand(
            CustomerName: "Amount Test",
            Email: "amount@test.com",
            PhoneNumber: "07700000001",
            Type: "collection",
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Item A", 10.00m, 2),
                new(Guid.NewGuid(), "Item B", 5.50m, 3)
            },
            Notes: null,
            DeliveryAddress: null,
            TableReservationName: null
        );

        var orderId = await _handler.Handle(cmd, CancellationToken.None);

        var order = await _dbContext.Orders.FindAsync(orderId);
        order!.TotalAmount.Should().Be(36.50m, "Total should be (10 × 2) + (5.50 × 3)");
    }

    [Fact]
    public async Task Handle_PersistsOrderItems_ToDatabase()
    {
        var orderId = await _handler.Handle(BuildCommand(), CancellationToken.None);

        var items = await _dbContext.OrderItems.Where(i => i.OrderId == orderId).ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.MenuItemName == "Momos");
        items.Should().Contain(i => i.MenuItemName == "Sel Roti");
    }

    [Fact]
    public async Task Handle_WithNotes_PersistsNotes()
    {
        var cmd = new CreateOrderCommand(
            CustomerName: "Notes Customer",
            Email: "notes@test.com",
            PhoneNumber: "07700000002",
            Type: "collection",
            Items: new List<CreateOrderItemDto> { new(Guid.NewGuid(), "Item", 5m, 1) },
            Notes: "Extra spicy please",
            DeliveryAddress: null,
            TableReservationName: null
        );

        var orderId = await _handler.Handle(cmd, CancellationToken.None);

        var order = await _dbContext.Orders.FindAsync(orderId);
        order!.Notes.Should().Be("Extra spicy please");
    }

    [Fact]
    public async Task Handle_WithDeliveryAddress_PersistsAddress()
    {
        var cmd = new CreateOrderCommand(
            CustomerName: "Delivery Customer",
            Email: "delivery@test.com",
            PhoneNumber: "07700000003",
            Type: "delivery",
            Items: new List<CreateOrderItemDto> { new(Guid.NewGuid(), "Item", 5m, 1) },
            Notes: null,
            DeliveryAddress: "123 Test Street, London",
            TableReservationName: null
        );

        var orderId = await _handler.Handle(cmd, CancellationToken.None);

        var order = await _dbContext.Orders.FindAsync(orderId);
        order!.DeliveryAddress.Should().Be("123 Test Street, London");
    }
}
