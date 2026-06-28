using FluentAssertions;
using Moq;
using NaarNoor.Application.Orders.Commands.CreateOrder;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using Xunit;

namespace NaarNoor.Application.Tests.Orders;

/// <summary>
/// Property-based tests for CreateOrderCommandHandler.
///
/// Validates that the handler:
/// 1. Completes without throwing for any valid command
/// 2. Maps customer fields from command to Order entity
/// 3. Maps OrderType correctly from type string ("delivery", "dine-in", anything else → collection)
/// 4. Calculates TotalAmount as sum(UnitPrice * Quantity)
/// 5. Calls SaveChangesAsync exactly once
/// 6. Returns a valid non-empty Guid
/// </summary>
public class CreateOrderCommandHandlerPropertyTests : ApplicationLayerTestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateOrderCommandHandler _handler;
    private readonly MockOrderRepository _mockRepository;

    public CreateOrderCommandHandlerPropertyTests()
    {
        _unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();
        _mockRepository = new MockOrderRepository();

        _unitOfWorkMock.Setup(x => x.Orders).Returns(_mockRepository);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new CreateOrderCommandHandler(_unitOfWorkMock.Object);
    }

    /// <summary>
    /// Property: Valid commands complete without throwing.
    /// </summary>
    [Theory(DisplayName = "Property: Valid commands complete without throwing")]
    [MemberData(nameof(GetValidOrderCommands))]
    public async Task Handle_WithValidCommand_CompletesSuccessfully(CreateOrderCommand command)
    {
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync("Valid orders should always be processed without error");
    }

    /// <summary>
    /// Property: Handler returns a valid non-empty Guid.
    /// </summary>
    [Theory(DisplayName = "Property: Handler returns a valid Guid")]
    [MemberData(nameof(GetValidOrderCommands))]
    public async Task Handle_WithValidCommand_ReturnsNonEmptyGuid(CreateOrderCommand command)
    {
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty, "Order ID must be a valid non-empty Guid");
    }

    /// <summary>
    /// Property: Customer fields are mapped correctly.
    /// </summary>
    [Theory(DisplayName = "Property: Customer fields are mapped from command")]
    [MemberData(nameof(GetValidOrderCommands))]
    public async Task Handle_WithValidCommand_MapsCustomerFieldsCorrectly(CreateOrderCommand command)
    {
        var capturedOrders = new List<Order>();
        var captureRepo = new MockOrderRepository();
        captureRepo.OnAdd = order => capturedOrders.Add(order);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(captureRepo);

        await _handler.Handle(command, CancellationToken.None);

        capturedOrders.Should().HaveCount(1);
        var saved = capturedOrders[0];

        saved.CustomerName.Should().Be(command.CustomerName);
        saved.Email.Should().Be(command.Email);
        saved.PhoneNumber.Should().Be(command.PhoneNumber);
        saved.Notes.Should().Be(command.Notes);
        saved.DeliveryAddress.Should().Be(command.DeliveryAddress);
        saved.TableReservationName.Should().Be(command.TableReservationName);
    }

    /// <summary>
    /// Property: OrderType is mapped correctly from the Type string.
    /// "delivery" → Delivery, "dine-in" → DineIn, anything else → Collection.
    /// </summary>
    [Theory(DisplayName = "Property: OrderType string is parsed correctly")]
    [InlineData("delivery", OrderType.Delivery)]
    [InlineData("DELIVERY", OrderType.Delivery)]
    [InlineData("Delivery", OrderType.Delivery)]
    [InlineData("dine-in", OrderType.DineIn)]
    [InlineData("DINE-IN", OrderType.DineIn)]
    [InlineData("collection", OrderType.Collection)]
    [InlineData("Collection", OrderType.Collection)]
    [InlineData("takeaway", OrderType.Collection)]
    [InlineData("unknown", OrderType.Collection)]
    public async Task Handle_MapsOrderTypeFromString(string typeString, OrderType expectedType)
    {
        // Arrange
        var capturedOrders = new List<Order>();
        var captureRepo = new MockOrderRepository();
        captureRepo.OnAdd = o => capturedOrders.Add(o);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(captureRepo);

        var command = BuildMinimalCommand(typeString);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedOrders.Should().HaveCount(1);
        capturedOrders[0].Type.Should().Be(expectedType,
            $"Type string '{typeString}' should map to {expectedType}");
    }

    /// <summary>
    /// Property: TotalAmount equals sum(UnitPrice * Quantity) for all items.
    /// </summary>
    [Theory(DisplayName = "Property: TotalAmount is calculated as sum of line totals")]
    [MemberData(nameof(GetValidOrderCommands))]
    public async Task Handle_CalculatesTotalAmountCorrectly(CreateOrderCommand command)
    {
        var capturedOrders = new List<Order>();
        var captureRepo = new MockOrderRepository();
        captureRepo.OnAdd = o => capturedOrders.Add(o);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(captureRepo);

        await _handler.Handle(command, CancellationToken.None);

        var expectedTotal = command.Items.Sum(i => i.UnitPrice * i.Quantity);
        capturedOrders[0].TotalAmount.Should().Be(expectedTotal,
            "TotalAmount should equal the sum of (UnitPrice * Quantity) for every item");
    }

    /// <summary>
    /// Property: SaveChangesAsync is called exactly once per order.
    /// </summary>
    [Theory(DisplayName = "Property: SaveChangesAsync is called exactly once")]
    [MemberData(nameof(GetValidOrderCommands))]
    public async Task Handle_SavesChangesExactlyOnce(CreateOrderCommand command)
    {
        await _handler.Handle(command, CancellationToken.None);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Property: Add is called before SaveChangesAsync.
    /// </summary>
    [Theory(DisplayName = "Property: Add is called before SaveChangesAsync")]
    [MemberData(nameof(GetValidOrderCommands))]
    public async Task Handle_CallsAddBeforeSave(CreateOrderCommand command)
    {
        var callOrder = new List<string>();
        var orderRepo = new MockOrderRepository();
        orderRepo.OnAdd = _ => callOrder.Add("Add");

        _unitOfWorkMock.Setup(x => x.Orders).Returns(orderRepo);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        callOrder.Should().HaveCount(2);
        callOrder[0].Should().Be("Add");
        callOrder[1].Should().Be("SaveChanges");
    }

    /// <summary>
    /// Property: Order items are persisted with correct fields.
    /// </summary>
    [Theory(DisplayName = "Property: Order items are mapped from command items")]
    [MemberData(nameof(GetValidOrderCommands))]
    public async Task Handle_PersistsOrderItemsCorrectly(CreateOrderCommand command)
    {
        var capturedOrders = new List<Order>();
        var captureRepo = new MockOrderRepository();
        captureRepo.OnAdd = o => capturedOrders.Add(o);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(captureRepo);

        await _handler.Handle(command, CancellationToken.None);

        var savedItems = capturedOrders[0].Items;
        savedItems.Should().HaveCount(command.Items.Count);

        for (int i = 0; i < command.Items.Count; i++)
        {
            savedItems[i].MenuItemId.Should().Be(command.Items[i].MenuItemId);
            savedItems[i].MenuItemName.Should().Be(command.Items[i].MenuItemName);
            savedItems[i].UnitPrice.Should().Be(command.Items[i].UnitPrice);
            savedItems[i].Quantity.Should().Be(command.Items[i].Quantity);
        }
    }

    #region Helpers

    private static CreateOrderCommand BuildMinimalCommand(string type) =>
        new CreateOrderCommand(
            CustomerName: "Test Customer",
            Email: "test@example.com",
            PhoneNumber: "07700000000",
            Notes: null,
            Type: type,
            DeliveryAddress: null,
            TableReservationName: null,
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Dal Bhat", 9.99m, 1)
            });

    public static TheoryData<CreateOrderCommand> GetValidOrderCommands()
    {
        var data = new TheoryData<CreateOrderCommand>();

        // Single delivery item
        data.Add(new CreateOrderCommand(
            CustomerName: "Priya Sharma",
            Email: "priya@example.com",
            PhoneNumber: "07700900001",
            Notes: "Extra spicy please",
            Type: "delivery",
            DeliveryAddress: "12 Himalayan Way, London, SW1A 1AA",
            TableReservationName: null,
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Lamb Rogan Josh", 14.99m, 2),
                new(Guid.NewGuid(), "Garlic Naan",     2.50m,  3)
            }));

        // Collection order
        data.Add(new CreateOrderCommand(
            CustomerName: "James Whitfield",
            Email: "james@whitfield.co.uk",
            PhoneNumber: "01234567890",
            Notes: null,
            Type: "collection",
            DeliveryAddress: null,
            TableReservationName: null,
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Chicken Tikka Masala", 12.99m, 1)
            }));

        // Dine-in order
        data.Add(new CreateOrderCommand(
            CustomerName: "Sara Magar",
            Email: "sara.magar@email.com",
            PhoneNumber: "07911123456",
            Notes: "Window table",
            Type: "dine-in",
            DeliveryAddress: null,
            TableReservationName: "Magar",
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Momo",          8.50m, 2),
                new(Guid.NewGuid(), "Thakali Khana", 16.00m, 1),
                new(Guid.NewGuid(), "Masala Tea",     2.00m, 2)
            }));

        // Multiple items — total calculation
        data.Add(new CreateOrderCommand(
            CustomerName: "Oliver Rana",
            Email: "oliver.rana@test.com",
            PhoneNumber: "07500001234",
            Notes: null,
            Type: "delivery",
            DeliveryAddress: "99 Everest Road, Manchester, M1 1AE",
            TableReservationName: null,
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Biryani",    13.99m, 3),
                new(Guid.NewGuid(), "Raita",       1.50m, 3),
                new(Guid.NewGuid(), "Mango Lassi", 3.50m, 2)
            }));

        return data;
    }

    #endregion

    #region Mock Repository

    private class MockOrderRepository : IRepository<Order>
    {
        public Action<Order>? OnAdd { get; set; }

        public void Add(Order entity) => OnAdd?.Invoke(entity);

        public void Remove(Order entity) => throw new NotImplementedException();

        public void Update(Order entity) => throw new NotImplementedException();

        public IQueryable<Order> Query() =>
            Enumerable.Empty<Order>().AsQueryable();
    }

    #endregion
}
