using FluentAssertions;
using Moq;
using NaarNoor.Application.Contact.Commands.SubmitInquiry;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using Xunit;

namespace NaarNoor.Application.Tests.Contact;

/// <summary>
/// Property-based tests for SubmitInquiryCommandHandler.
///
/// Validates that the handler:
/// 1. Completes without throwing for any valid command
/// 2. Persists a ContactInquiry with fields matching the command
/// 3. Returns a valid non-empty Guid
/// 4. Calls SaveChangesAsync exactly once
/// 5. Calls Add before SaveChangesAsync (transaction order)
/// </summary>
public class SubmitInquiryCommandHandlerPropertyTests : ApplicationLayerTestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SubmitInquiryCommandHandler _handler;
    private readonly MockContactInquiryRepository _mockRepository;

    public SubmitInquiryCommandHandlerPropertyTests()
    {
        _unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();
        _mockRepository = new MockContactInquiryRepository();

        _unitOfWorkMock.Setup(x => x.ContactInquiries).Returns(_mockRepository);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new SubmitInquiryCommandHandler(_unitOfWorkMock.Object);
    }

    /// <summary>
    /// Property: Valid commands complete without throwing.
    /// </summary>
    [Theory(DisplayName = "Property: Valid commands complete without throwing")]
    [MemberData(nameof(GetValidInquiryCommands))]
    public async Task Handle_WithValidCommand_CompletesSuccessfully(SubmitInquiryCommand command)
    {
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync("Valid commands should always complete successfully");
    }

    /// <summary>
    /// Property: Handler returns a valid non-empty Guid.
    /// </summary>
    [Theory(DisplayName = "Property: Handler returns a valid non-empty Guid")]
    [MemberData(nameof(GetValidInquiryCommands))]
    public async Task Handle_WithValidCommand_ReturnsNonEmptyGuid(SubmitInquiryCommand command)
    {
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty, "Returned ID should be a valid non-empty Guid");
    }

    /// <summary>
    /// Property: Repository Add is called with entity fields matching the command.
    /// </summary>
    [Theory(DisplayName = "Property: Repository captures correct entity data")]
    [MemberData(nameof(GetValidInquiryCommands))]
    public async Task Handle_WithValidCommand_PersistsCorrectEntityData(SubmitInquiryCommand command)
    {
        // Arrange - fresh repository to capture added entity
        var capturedInquiries = new List<ContactInquiry>();
        var captureRepo = new MockContactInquiryRepository();
        captureRepo.OnAdd = inquiry => capturedInquiries.Add(inquiry);

        _unitOfWorkMock.Setup(x => x.ContactInquiries).Returns(captureRepo);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - exactly one entity persisted
        capturedInquiries.Should().HaveCount(1, "Add should be called exactly once");

        var saved = capturedInquiries[0];
        saved.Name.Should().Be(command.Name, "Name must match command");
        saved.Email.Should().Be(command.Email, "Email must match command");
        saved.PhoneNumber.Should().Be(command.PhoneNumber, "PhoneNumber must match command");
        saved.Subject.Should().Be(command.Subject, "Subject must match command");
        saved.Message.Should().Be(command.Message, "Message must match command");
    }

    /// <summary>
    /// Property: SaveChangesAsync is called exactly once.
    /// </summary>
    [Theory(DisplayName = "Property: SaveChangesAsync called exactly once")]
    [MemberData(nameof(GetValidInquiryCommands))]
    public async Task Handle_WithValidCommand_SavesChangesExactlyOnce(SubmitInquiryCommand command)
    {
        await _handler.Handle(command, CancellationToken.None);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "SaveChangesAsync should be called exactly once per command");
    }

    /// <summary>
    /// Property: Add is called before SaveChangesAsync (correct transaction order).
    /// </summary>
    [Theory(DisplayName = "Property: Add is called before SaveChangesAsync")]
    [MemberData(nameof(GetValidInquiryCommands))]
    public async Task Handle_WithValidCommand_CallsAddBeforeSave(SubmitInquiryCommand command)
    {
        // Arrange
        var callOrder = new List<string>();
        var orderRepo = new MockContactInquiryRepository();
        orderRepo.OnAdd = _ => callOrder.Add("Add");

        _unitOfWorkMock.Setup(x => x.ContactInquiries).Returns(orderRepo);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().HaveCount(2);
        callOrder[0].Should().Be("Add", "Add must be called before SaveChanges");
        callOrder[1].Should().Be("SaveChanges", "SaveChanges must be called after Add");
    }

    /// <summary>
    /// Property: PhoneNumber may be null and handler still succeeds.
    /// </summary>
    [Fact(DisplayName = "Property: Null PhoneNumber is accepted")]
    public async Task Handle_WithNullPhoneNumber_Succeeds()
    {
        var command = new SubmitInquiryCommand(
            Name: "Anonymous",
            Email: "anon@example.com",
            PhoneNumber: null,
            Subject: "General Enquiry",
            Message: "Hello, I have a question.");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    #region Test Data

    public static TheoryData<SubmitInquiryCommand> GetValidInquiryCommands()
    {
        var data = new TheoryData<SubmitInquiryCommand>();

        data.Add(new SubmitInquiryCommand(
            Name: "Alice Johnson",
            Email: "alice@example.com",
            PhoneNumber: "07700900001",
            Subject: "Reservation Enquiry",
            Message: "I would like to book a table for 10 people."));

        data.Add(new SubmitInquiryCommand(
            Name: "Bob Smith",
            Email: "bob.smith@corporate.co.uk",
            PhoneNumber: null,
            Subject: "Allergy Information",
            Message: "Please advise on gluten-free options on your menu."));

        data.Add(new SubmitInquiryCommand(
            Name: "Chloé Dupont",
            Email: "chloe@email.fr",
            PhoneNumber: "+33 6 12 34 56 78",
            Subject: "Private Event",
            Message: "We are interested in hosting a private dinner for 30 guests."));

        data.Add(new SubmitInquiryCommand(
            Name: "David Lama",
            Email: "david@trekking.com",
            PhoneNumber: "01234567890",
            Subject: "Catering",
            Message: "Could you provide catering for our annual summit?"));

        data.Add(new SubmitInquiryCommand(
            Name: "Eva Rai",
            Email: "eva.rai@naartest.com",
            PhoneNumber: "+977 98 1234 5678",
            Subject: "Feedback",
            Message: "Wonderful experience last weekend. The lamb was exceptional."));

        return data;
    }

    #endregion

    #region Mock Repository

    private class MockContactInquiryRepository : IRepository<ContactInquiry>
    {
        public Action<ContactInquiry>? OnAdd { get; set; }

        public void Add(ContactInquiry entity) => OnAdd?.Invoke(entity);

        public void Remove(ContactInquiry entity) => throw new NotImplementedException();

        public void Update(ContactInquiry entity) => throw new NotImplementedException();

        public IQueryable<ContactInquiry> Query() =>
            Enumerable.Empty<ContactInquiry>().AsQueryable();
    }

    #endregion
}
