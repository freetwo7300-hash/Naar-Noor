using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Application.Reservations.Commands.CreateReservation;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Application.Tests.Reservations.Commands;

public class CreateReservationCommandHandlerTests : IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private CreateReservationCommandHandler _handler = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("CreateRes_" + Guid.NewGuid())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _unitOfWork = new UnitOfWork(_dbContext);
        _handler = new CreateReservationCommandHandler(_unitOfWork);
    }

    public async Task DisposeAsync() => await _dbContext.DisposeAsync();

    private static CreateReservationCommand BuildCommand(
        string customerName = "Test Guest",
        string email = "guest@test.com",
        int partySize = 2,
        string? specialRequests = null) =>
        new CreateReservationCommand(
            CustomerName: customerName,
            Email: email,
            PhoneNumber: "07700000000",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            ReservationTime: "19:00",
            PartySize: partySize,
            SpecialRequests: specialRequests
        );

    [Fact]
    public async Task Handle_ReturnsNewReservationId()
    {
        var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

        result.Should().NotBe(Guid.Empty, "Handler should return a valid Reservation ID");
    }

    [Fact]
    public async Task Handle_PersistsReservationToDatabase()
    {
        var id = await _handler.Handle(BuildCommand(), CancellationToken.None);

        var reservation = await _dbContext.Reservations.FindAsync(id);
        reservation.Should().NotBeNull();
        reservation!.CustomerName.Should().Be("Test Guest");
        reservation.Email.Should().Be("guest@test.com");
        reservation.PartySize.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ParsesReservationTime_Correctly()
    {
        var cmd = BuildCommand();
        var id = await _handler.Handle(cmd, CancellationToken.None);

        var reservation = await _dbContext.Reservations.FindAsync(id);
        reservation!.ReservationTime.Should().Be(new TimeOnly(19, 0));
    }

    [Fact]
    public async Task Handle_WithSpecialRequests_PersistsThem()
    {
        var id = await _handler.Handle(BuildCommand(specialRequests: "Nut allergy"), CancellationToken.None);

        var reservation = await _dbContext.Reservations.FindAsync(id);
        reservation!.SpecialRequests.Should().Be("Nut allergy");
    }

    [Fact]
    public async Task Handle_WithNoSpecialRequests_ReservationIsStillSaved()
    {
        var id = await _handler.Handle(BuildCommand(specialRequests: null), CancellationToken.None);

        var reservation = await _dbContext.Reservations.FindAsync(id);
        reservation.Should().NotBeNull();
        reservation!.SpecialRequests.Should().BeNull();
    }

    [Fact]
    public async Task Handle_LargePartySize_PersistsCorrectly()
    {
        var id = await _handler.Handle(BuildCommand(partySize: 12), CancellationToken.None);

        var reservation = await _dbContext.Reservations.FindAsync(id);
        reservation!.PartySize.Should().Be(12);
    }

    [Fact]
    public async Task Handle_MultipleReservations_EachGetUniqueId()
    {
        var id1 = await _handler.Handle(BuildCommand("Guest A", "a@test.com"), CancellationToken.None);
        var id2 = await _handler.Handle(BuildCommand("Guest B", "b@test.com"), CancellationToken.None);

        id1.Should().NotBe(id2, "Each reservation should have a unique ID");
        var count = await _dbContext.Reservations.CountAsync();
        count.Should().Be(2);
    }
}
