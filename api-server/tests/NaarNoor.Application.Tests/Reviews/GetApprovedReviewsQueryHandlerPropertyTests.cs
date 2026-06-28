using FluentAssertions;
using Moq;
using NaarNoor.Application.Reviews.Queries.GetApprovedReviews;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using Xunit;

namespace NaarNoor.Application.Tests.Reviews;

/// <summary>
/// Property-based tests for GetApprovedReviewsQueryHandler.
///
/// Validates that the handler:
/// 1. Returns only approved reviews
/// 2. Orders results by CreatedAt descending (newest first)
/// 3. Transforms entities to DTOs with all fields correctly mapped
/// 4. Never calls SaveChangesAsync (read-only)
/// 5. Returns empty list when no approved reviews exist
/// </summary>
public class GetApprovedReviewsQueryHandlerPropertyTests : ApplicationLayerTestBase
{
    /// <summary>
    /// Property: Only approved reviews returned, ordered newest-first.
    /// </summary>
    [Theory(DisplayName = "Property: Only approved reviews returned in descending CreatedAt order")]
    [MemberData(nameof(GetMixedApprovalDatasets))]
    public async Task Handle_ReturnsOnlyApprovedReviewsNewestFirst(List<Review> reviews)
    {
        var unitOfWorkMock = CreateUnitOfWorkMockWithReviews(reviews);
        var handler = new GetApprovedReviewsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        // Only approved reviews
        var expectedCount = reviews.Count(r => r.IsApproved);
        result.Should().HaveCount(expectedCount, "Only approved reviews should be returned");

        // Descending by CreatedAt
        if (result.Count > 1)
        {
            result.Select(r => r.CreatedAt)
                  .Should().BeInDescendingOrder("Results must be ordered by CreatedAt descending");
        }

        // No writes on a query
        unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "Query handler must not persist any changes");
    }

    /// <summary>
    /// Property: DTO fields are mapped exactly from Review entity fields.
    /// </summary>
    [Theory(DisplayName = "Property: DTO fields match entity fields exactly")]
    [MemberData(nameof(GetApprovedOnlyDatasets))]
    public async Task Handle_MapsAllReviewFieldsCorrectly(List<Review> approvedReviews)
    {
        var ordered = approvedReviews.OrderByDescending(r => r.CreatedAt).ToList();
        var unitOfWorkMock = CreateUnitOfWorkMockWithReviews(approvedReviews);
        var handler = new GetApprovedReviewsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result.Should().HaveCount(ordered.Count);
        for (int i = 0; i < result.Count; i++)
        {
            var dto = result[i];
            var entity = ordered[i];

            dto.Id.Should().Be(entity.Id, "Id must match");
            dto.CustomerName.Should().Be(entity.CustomerName, "CustomerName must match");
            dto.Rating.Should().Be(entity.Rating, "Rating must match");
            dto.Comment.Should().Be(entity.Comment, "Comment must match");
            dto.Source.Should().Be(entity.Source, "Source must match");
            dto.CreatedAt.Should().Be(entity.CreatedAt, "CreatedAt must match");
        }
    }

    /// <summary>
    /// Property: Returns empty list when no approved reviews exist.
    /// </summary>
    [Fact(DisplayName = "Property: Empty list when no approved reviews exist")]
    public async Task Handle_WithNoApprovedReviews_ReturnsEmptyList()
    {
        var pending = Enumerable.Range(0, 5).Select(i => new Review
        {
            CustomerName = $"Customer {i}",
            Rating = 3,
            Comment = "Needs review",
            IsApproved = false,
            Source = "Google",
            CreatedAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();

        var unitOfWorkMock = CreateUnitOfWorkMockWithReviews(pending);
        var handler = new GetApprovedReviewsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result.Should().BeEmpty("No approved reviews means empty result");
    }

    /// <summary>
    /// Property: Unapproved review IDs never appear in results.
    /// </summary>
    [Theory(DisplayName = "Property: Unapproved reviews are never returned")]
    [MemberData(nameof(GetMixedApprovalDatasets))]
    public async Task Handle_NeverReturnsUnapprovedReviews(List<Review> mixedReviews)
    {
        var unitOfWorkMock = CreateUnitOfWorkMockWithReviews(mixedReviews);
        var handler = new GetApprovedReviewsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        var unapprovedIds = mixedReviews.Where(r => !r.IsApproved).Select(r => r.Id).ToHashSet();
        result.Should().NotContain(dto => unapprovedIds.Contains(dto.Id),
            "Unapproved review IDs must never appear in results");
    }

    /// <summary>
    /// Property: Required DTO fields are always non-null / in valid range.
    /// </summary>
    [Theory(DisplayName = "Property: DTO required fields are always populated and valid")]
    [MemberData(nameof(GetApprovedOnlyDatasets))]
    public async Task Handle_ReturnsDtosWithAllRequiredFields(List<Review> approvedReviews)
    {
        var unitOfWorkMock = CreateUnitOfWorkMockWithReviews(approvedReviews);
        var handler = new GetApprovedReviewsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        foreach (var dto in result)
        {
            dto.Id.Should().NotBe(Guid.Empty, "Id must be non-empty");
            dto.CustomerName.Should().NotBeNullOrWhiteSpace("CustomerName must be populated");
            dto.Rating.Should().BeInRange(1, 5, "Rating must be 1–5");
            dto.Comment.Should().NotBeNullOrWhiteSpace("Comment must be populated");
            dto.CreatedAt.Should().NotBe(default(DateTime), "CreatedAt must be set");
        }
    }

    /// <summary>
    /// Property: Two identical calls on the same data return the same results.
    /// </summary>
    [Theory(DisplayName = "Property: Identical inputs produce identical outputs")]
    [MemberData(nameof(GetApprovedOnlyDatasets))]
    public async Task Handle_WithSameData_ReturnsConsistentResults(List<Review> approvedReviews)
    {
        var unitOfWorkMock1 = CreateUnitOfWorkMockWithReviews(approvedReviews.ToList());
        var unitOfWorkMock2 = CreateUnitOfWorkMockWithReviews(approvedReviews.ToList());

        var handler1 = new GetApprovedReviewsQueryHandler(unitOfWorkMock1.Object);
        var handler2 = new GetApprovedReviewsQueryHandler(unitOfWorkMock2.Object);

        var result1 = await handler1.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);
        var result2 = await handler2.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result1.Should().HaveCount(result2.Count);
        for (int i = 0; i < result1.Count; i++)
        {
            result1[i].Should().BeEquivalentTo(result2[i], "Same inputs must yield same outputs");
        }
    }

    #region Helpers

    private Mock<IUnitOfWork> CreateUnitOfWorkMockWithReviews(List<Review> reviews)
    {
        var unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();
        var repositoryMock = CreateRepositoryMock<IRepository<Review>>();

        repositoryMock.Setup(r => r.Query()).Returns(reviews.AsAsyncTestQueryable());
        unitOfWorkMock.Setup(u => u.Reviews).Returns(repositoryMock.Object);
        unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        return unitOfWorkMock;
    }

    public static TheoryData<List<Review>> GetMixedApprovalDatasets()
    {
        var data = new TheoryData<List<Review>>();

        data.Add(new List<Review>
        {
            new() { CustomerName = "Alex",   Rating = 5, Comment = "Outstanding!",       IsApproved = true,  Source = "Google",      CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { CustomerName = "Blake",  Rating = 2, Comment = "Needs improvement",  IsApproved = false, Source = "TripAdvisor", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { CustomerName = "Cara",   Rating = 4, Comment = "Really good food",   IsApproved = true,  Source = "Facebook",    CreatedAt = DateTime.UtcNow.AddDays(-3) }
        });

        data.Add(new List<Review>
        {
            new() { CustomerName = "Dan",    Rating = 1, Comment = "Very disappointed",  IsApproved = false, Source = null,          CreatedAt = DateTime.UtcNow.AddDays(-4) }
        });

        data.Add(new List<Review>
        {
            new() { CustomerName = "Emma",   Rating = 5, Comment = "Perfect experience!", IsApproved = true,  Source = "Yelp", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { CustomerName = "Faisal", Rating = 4, Comment = "Great momo!",         IsApproved = true,  Source = null,   CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { CustomerName = "Grace",  Rating = 1, Comment = "Bad service",          IsApproved = false, Source = "Google", CreatedAt = DateTime.UtcNow.AddDays(-2) }
        });

        return data;
    }

    public static TheoryData<List<Review>> GetApprovedOnlyDatasets()
    {
        var data = new TheoryData<List<Review>>();

        data.Add(new List<Review>
        {
            new() { CustomerName = "Emma",   Rating = 5, Comment = "Authentic Himalayan flavours!", IsApproved = true, Source = "Google",   CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { CustomerName = "Faisal", Rating = 4, Comment = "Great momos, recommend.",       IsApproved = true, Source = "Yelp",     CreatedAt = DateTime.UtcNow.AddDays(-5) }
        });

        data.Add(new List<Review>
        {
            new() { CustomerName = "Grace", Rating = 3, Comment = "Decent food, slow service.",    IsApproved = true, Source = null,       CreatedAt = DateTime.UtcNow.AddDays(-10) }
        });

        data.Add(new List<Review>
        {
            new() { CustomerName = "Hero A", Rating = 5, Comment = "Best restaurant in town!",     IsApproved = true, Source = "Facebook", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { CustomerName = "Hero B", Rating = 5, Comment = "Came back three times!",       IsApproved = true, Source = "Google",   CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new() { CustomerName = "Hero C", Rating = 4, Comment = "Loved the lamb rogan josh.",   IsApproved = true, Source = "TripAdvisor", CreatedAt = DateTime.UtcNow.AddDays(-14) }
        });

        return data;
    }

    #endregion
}
