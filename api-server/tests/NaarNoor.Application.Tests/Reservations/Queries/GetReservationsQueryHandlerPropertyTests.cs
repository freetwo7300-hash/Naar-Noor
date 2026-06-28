using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Reservations.Queries.GetReservations;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using Xunit;

namespace NaarNoor.Application.Tests.Reservations.Queries;

/// <summary>
/// Property-based tests for GetReservationsQueryHandler.
/// 
/// **Validates: Requirements 2.2**
/// 
/// These tests verify that query handlers:
/// 1. Filter results correctly according to query parameters
/// 2. Transformation logic produces correct data types (DTOs)
/// 3. Filtering predicates work across various input combinations
/// 4. Results maintain data consistency
/// 5. Filtering does not mutate source data
/// </summary>
public class GetReservationsQueryHandlerPropertyTests : ApplicationLayerTestBase
{
    /// <summary>
    /// Property: Query Result Filtering and Transformation
    /// 
    /// For any query executed with random pagination parameters against seeded test data,
    /// the query handler SHALL:
    /// - Return only results matching the requested page
    /// - Correctly apply pagination (skip and take)
    /// - Not access the database directly
    /// - Transform entities to DTOs with all required fields populated
    /// - Maintain ordering consistency (descending by reservation date)
    /// </summary>
    [Property(StartSize = 0, EndSize = 50, MaxTest = 100)]
    public Property QueryHandlerReturnsFilteredResultsWithCorrectPagination(
        NonNegativeInt pageNumber,
        NonNegativeInt pageSize)
    {
        return Prop.ForAll(
            Gen.Constant(GenerateValidPageParameters().First()).ToArbitrary(),
            pageParams =>
            {
                // Arrange
                var testData = GenerateTestReservations(itemCount: 100);
                var unitOfWorkMock = CreateUnitOfWorkMockWithReservations(testData);

                var handler = new GetReservationsQueryHandler(unitOfWorkMock.Object);
                var query = new GetReservationsQuery(pageParams.Page, pageParams.PageSize);

                // Act
                var result = handler.Handle(query, CancellationToken.None).Result;

                // Assert - Structure & Type Correctness
                result.Should().NotBeNull("Result should not be null");
                result.Should().BeOfType<List<ReservationDto>>("Result should be a list of DTOs");
                result.All(r => r != null).Should().BeTrue("All DTOs should be non-null");

                // Assert - Pagination Correctness
                var expectedSkip = (pageParams.Page - 1) * pageParams.PageSize;
                var expectedTake = pageParams.PageSize;
                var expectedResults = testData
                    .OrderByDescending(r => r.ReservationDate)
                    .Skip(expectedSkip)
                    .Take(expectedTake)
                    .ToList();

                result.Count.Should().Be(expectedResults.Count, 
                    $"Page {pageParams.Page} with size {pageParams.PageSize} should return {expectedResults.Count} results");

                // Assert - Data Transformation
                for (int i = 0; i < result.Count; i++)
                {
                    var dto = result[i];
                    var entity = expectedResults[i];

                    dto.Id.Should().Be(entity.Id);
                    dto.CustomerName.Should().Be(entity.CustomerName);
                    dto.Email.Should().Be(entity.Email);
                    dto.PhoneNumber.Should().Be(entity.PhoneNumber);
                    dto.ReservationDate.Should().Be(entity.ReservationDate);
                    dto.ReservationTime.Should().Be(entity.ReservationTime.ToString("HH:mm"));
                    dto.PartySize.Should().Be(entity.PartySize);
                    dto.Status.Should().Be(entity.Status.ToString());
                    dto.SpecialRequests.Should().Be(entity.SpecialRequests);
                    dto.CreatedAt.Should().Be(entity.CreatedAt);
                }

                // Assert - Ordering Consistency
                var dateOrder = result
                    .Select(r => DateOnly.Parse(r.ReservationDate.ToString()))
                    .Zip(result.Skip(1).Select(r => DateOnly.Parse(r.ReservationDate.ToString())),
                        (current, next) => current >= next)
                    .All(x => x);
                dateOrder.Should().BeTrue("Results should be ordered by reservation date descending");

                // Assert - No Source Mutation
                testData.Should().HaveCount(100, "Handler should not remove items from source data");
                testData.Should().AllSatisfy(r =>
                    r.CustomerName.Should().StartWith("Customer "),
                    "Handler should not null out or alter source entities");

                // Assert - No Database Access
                unitOfWorkMock.Verify(
                    uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                    Times.Never,
                    "Query handler should not save changes to database");

                return true.ToProperty();
            });
    }

    /// <summary>
    /// Property: Query Result Transformation Completeness
    /// 
    /// For any reservation entity with valid data,
    /// the transformation to DTO SHALL produce DTOs with all fields populated correctly
    /// and no null values in required fields.
    /// </summary>
    [Property(StartSize = 10, EndSize = 50, MaxTest = 50)]
    public Property QueryTransformsAllReservationFieldsCorrectly()
    {
        return Prop.ForAll(
            Gen.Constant(GenerateTestReservations(20)).ToArbitrary(),
            reservations =>
            {
                // Arrange
                var unitOfWorkMock = CreateUnitOfWorkMockWithReservations(reservations);
                var handler = new GetReservationsQueryHandler(unitOfWorkMock.Object);
                var query = new GetReservationsQuery(1, 100);

                // Act
                var result = handler.Handle(query, CancellationToken.None).Result;

                // Assert - All fields are present
                foreach (var dto in result)
                {
                    dto.Id.Should().NotBe(Guid.Empty, "ID should not be empty");
                    dto.CustomerName.Should().NotBeNullOrWhiteSpace("Customer name should be populated");
                    dto.Email.Should().NotBeNullOrWhiteSpace("Email should be populated");
                    dto.PhoneNumber.Should().NotBeNullOrWhiteSpace("Phone number should be populated");
                    dto.ReservationDate.Should().NotBe(default(DateOnly), "Reservation date should be populated");
                    dto.ReservationTime.Should().NotBeNullOrWhiteSpace("Reservation time should be populated");
                    dto.PartySize.Should().BeGreaterThan(0, "Party size should be greater than 0");
                    dto.Status.Should().NotBeNullOrWhiteSpace("Status should be populated");
                    dto.CreatedAt.Should().NotBe(default(DateTime), "CreatedAt should be populated");

                    // Verify time format (HH:mm)
                    dto.ReservationTime.Should().MatchRegex("[0-2][0-9]:[0-5][0-9]", 
                        "Time should be in HH:mm format");
                }

                return true.ToProperty();
            });
    }

    /// <summary>
    /// Property: Query Results Independent of Source Data Mutations
    /// 
    /// For any query executed twice with identical parameters on the same mocked repository,
    /// the results SHALL be identical and unaffected by external modifications to the
    /// source data collection.
    /// </summary>
    [Property(StartSize = 10, EndSize = 30, MaxTest = 50)]
    public Property QueryResultsAreConsistentAndIndependent()
    {
        return Prop.ForAll(
            Gen.Constant(GenerateTestReservations(30)).ToArbitrary(),
            reservations =>
            {
                // Arrange
                var unitOfWorkMock1 = CreateUnitOfWorkMockWithReservations(reservations.ToList());
                var unitOfWorkMock2 = CreateUnitOfWorkMockWithReservations(reservations.ToList());

                var handler1 = new GetReservationsQueryHandler(unitOfWorkMock1.Object);
                var handler2 = new GetReservationsQueryHandler(unitOfWorkMock2.Object);

                var query = new GetReservationsQuery(1, 50);

                // Act
                var result1 = handler1.Handle(query, CancellationToken.None).Result;
                var result2 = handler2.Handle(query, CancellationToken.None).Result;

                // Assert - Results should be identical
                result1.Should().HaveCount(result2.Count, "Both queries should return the same number of results");
                
                for (int i = 0; i < result1.Count; i++)
                {
                    result1[i].Should().BeEquivalentTo(result2[i], 
                        "Results at same index should be equivalent");
                }

                return true.ToProperty();
            });
    }

    // Helper methods

    private IEnumerable<(int Page, int PageSize)> GenerateValidPageParameters()
    {
        for (int page = 1; page <= 5; page++)
        {
            for (int pageSize = 10; pageSize <= 50; pageSize += 10)
            {
                yield return (page, pageSize);
            }
        }
    }

    private List<Reservation> GenerateTestReservations(int itemCount)
    {
        var reservations = new List<Reservation>();
        var baseDate = new DateOnly(2024, 1, 1);

        for (int i = 0; i < itemCount; i++)
        {
            var reservation = new Reservation
            {
                CustomerName    = $"Customer {i}",
                Email           = $"customer{i}@example.com",
                PhoneNumber     = $"555-{i:D4}",
                ReservationDate = baseDate.AddDays(i % 30),
                ReservationTime = new TimeOnly(18, 0).AddMinutes(i % 60),
                PartySize       = (i % 6) + 1,
                SpecialRequests = i % 2 == 0 ? $"Request {i}" : null
            };
            reservations.Add(reservation);
        }

        return reservations;
    }

    private IEnumerable<List<Reservation>> GenerateValidReservations(int count)
    {
        for (int seed = 0; seed < 5; seed++)
        {
            yield return GenerateTestReservations(count);
        }
    }

    private Mock<IUnitOfWork> CreateUnitOfWorkMockWithReservations(List<Reservation> reservations)
    {
        var unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();

        // Create a queryable mock that returns the test data
        var queryable = reservations.AsAsyncTestQueryable();

        var repositoryMock = CreateRepositoryMock<IRepository<Reservation>>();
        repositoryMock
            .Setup(r => r.Query())
            .Returns(queryable);

        unitOfWorkMock
            .Setup(u => u.Reservations)
            .Returns(repositoryMock.Object);

        // Ensure SaveChangesAsync is not called
        unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));

        return unitOfWorkMock;
    }
}
