using FluentAssertions;
using Moq;
using NaarNoor.Application.Chefs.Queries.GetChefs;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using Xunit;

namespace NaarNoor.Application.Tests.Chefs;

/// <summary>
/// Property-based tests for GetChefsQueryHandler.
///
/// Validates that the handler:
/// 1. Returns only active chefs
/// 2. Orders results by SortOrder ascending
/// 3. Transforms entities to DTOs correctly
/// 4. Never calls SaveChangesAsync (read-only)
/// 5. Returns empty list when no active chefs exist
/// </summary>
public class GetChefsQueryHandlerPropertyTests : ApplicationLayerTestBase
{
    /// <summary>
    /// Property: Only active chefs are returned, ordered by SortOrder.
    /// </summary>
    [Theory(DisplayName = "Property: Only active chefs returned in SortOrder order")]
    [MemberData(nameof(GetMixedActiveInactiveChefDatasets))]
    public async Task Handle_ReturnsOnlyActiveChefsSortedBySortOrder(List<Chef> chefs)
    {
        var unitOfWorkMock = CreateUnitOfWorkMockWithChefs(chefs);
        var handler = new GetChefsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetChefsQuery(), CancellationToken.None);

        // Only active chefs
        var expectedCount = chefs.Count(c => c.IsActive);
        result.Should().HaveCount(expectedCount, "Only active chefs should be returned");

        // Ordered by SortOrder ascending
        result.Select(c => c.SortOrder).Should().BeInAscendingOrder(
            "Results should be ordered by SortOrder ascending");

        // No SaveChangesAsync on a query
        unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "Query handler must not persist any changes");
    }

    /// <summary>
    /// Property: DTO fields are mapped correctly from entity fields.
    /// </summary>
    [Theory(DisplayName = "Property: DTO fields match entity fields exactly")]
    [MemberData(nameof(GetPureActiveChefDatasets))]
    public async Task Handle_MapsAllChefFieldsCorrectly(List<Chef> activeChefs)
    {
        var ordered = activeChefs.OrderBy(c => c.SortOrder).ToList();
        var unitOfWorkMock = CreateUnitOfWorkMockWithChefs(ordered);
        var handler = new GetChefsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetChefsQuery(), CancellationToken.None);

        result.Should().HaveCount(ordered.Count);
        for (int i = 0; i < result.Count; i++)
        {
            var dto = result[i];
            var entity = ordered[i];

            dto.Id.Should().Be(entity.Id, "Id must match");
            dto.Name.Should().Be(entity.Name, "Name must match");
            dto.Title.Should().Be(entity.Title, "Title must match");
            dto.Bio.Should().Be(entity.Bio, "Bio must match");
            dto.ImageUrl.Should().Be(entity.ImageUrl, "ImageUrl must match");
            dto.Specialty.Should().Be(entity.Specialty, "Specialty must match");
            dto.SortOrder.Should().Be(entity.SortOrder, "SortOrder must match");
        }
    }

    /// <summary>
    /// Property: Handler returns empty list when no active chefs exist.
    /// </summary>
    [Fact(DisplayName = "Property: Empty list when all chefs inactive")]
    public async Task Handle_WithNoActiveChefs_ReturnsEmptyList()
    {
        var inactiveChefs = BuildChefs(5, isActive: false);
        var unitOfWorkMock = CreateUnitOfWorkMockWithChefs(inactiveChefs);
        var handler = new GetChefsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetChefsQuery(), CancellationToken.None);

        result.Should().BeEmpty("No active chefs means empty result");
    }

    /// <summary>
    /// Property: Required DTO fields are always non-null / non-empty.
    /// </summary>
    [Theory(DisplayName = "Property: DTO required fields are always populated")]
    [MemberData(nameof(GetPureActiveChefDatasets))]
    public async Task Handle_ReturnsDtosWithAllRequiredFields(List<Chef> activeChefs)
    {
        var unitOfWorkMock = CreateUnitOfWorkMockWithChefs(activeChefs);
        var handler = new GetChefsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetChefsQuery(), CancellationToken.None);

        foreach (var dto in result)
        {
            dto.Id.Should().NotBe(Guid.Empty, "Id must be non-empty");
            dto.Name.Should().NotBeNullOrWhiteSpace("Name must be populated");
            dto.Title.Should().NotBeNullOrWhiteSpace("Title must be populated");
            dto.Bio.Should().NotBeNullOrWhiteSpace("Bio must be populated");
            dto.Specialty.Should().NotBeNullOrWhiteSpace("Specialty must be populated");
            dto.SortOrder.Should().BeGreaterThanOrEqualTo(0, "SortOrder must be non-negative");
        }
    }

    /// <summary>
    /// Property: Inactive chefs are never included, regardless of other field values.
    /// </summary>
    [Theory(DisplayName = "Property: Inactive chefs never appear in results")]
    [MemberData(nameof(GetMixedActiveInactiveChefDatasets))]
    public async Task Handle_NeverReturnsInactiveChefs(List<Chef> mixedChefs)
    {
        var unitOfWorkMock = CreateUnitOfWorkMockWithChefs(mixedChefs);
        var handler = new GetChefsQueryHandler(unitOfWorkMock.Object);

        var result = await handler.Handle(new GetChefsQuery(), CancellationToken.None);

        var inactiveIds = mixedChefs.Where(c => !c.IsActive).Select(c => c.Id).ToHashSet();
        result.Should().NotContain(dto => inactiveIds.Contains(dto.Id),
            "Inactive chef IDs must not appear in results");
    }

    /// <summary>
    /// Property: SaveChangesAsync is never called (queries are read-only).
    /// </summary>
    [Theory(DisplayName = "Property: SaveChangesAsync never called for a query")]
    [MemberData(nameof(GetPureActiveChefDatasets))]
    public async Task Handle_NeverCallsSaveChanges(List<Chef> chefs)
    {
        var unitOfWorkMock = CreateUnitOfWorkMockWithChefs(chefs);
        var handler = new GetChefsQueryHandler(unitOfWorkMock.Object);

        await handler.Handle(new GetChefsQuery(), CancellationToken.None);

        unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #region Helpers

    private static List<Chef> BuildChefs(int count, bool isActive = true) =>
        Enumerable.Range(0, count).Select(i => new Chef
        {
            Name = $"Chef {i}",
            Title = i % 2 == 0 ? "Head Chef" : "Sous Chef",
            Bio = $"Biography {i}",
            ImageUrl = i % 3 == 0 ? null : $"https://example.com/chef{i}.jpg",
            Specialty = i % 2 == 0 ? "Himalayan" : "Nepalese",
            IsActive = isActive,
            SortOrder = i
        }).ToList();

    private Mock<IUnitOfWork> CreateUnitOfWorkMockWithChefs(List<Chef> chefs)
    {
        var unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();
        var repositoryMock = CreateRepositoryMock<IRepository<Chef>>();

        repositoryMock.Setup(r => r.Query()).Returns(chefs.AsAsyncTestQueryable());
        unitOfWorkMock.Setup(u => u.Chefs).Returns(repositoryMock.Object);
        unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        return unitOfWorkMock;
    }

    public static TheoryData<List<Chef>> GetPureActiveChefDatasets()
    {
        var data = new TheoryData<List<Chef>>();

        data.Add(new List<Chef>
        {
            new() { Name = "Roshan Thapa",  Title = "Head Chef",      Bio = "15 years Himalayan cuisine",    Specialty = "Himalayan", IsActive = true, SortOrder = 1 },
            new() { Name = "Anita Gurung",  Title = "Sous Chef",       Bio = "Nepalese street food specialist", Specialty = "Nepalese",  IsActive = true, SortOrder = 2 },
            new() { Name = "Karma Sherpa",  Title = "Pastry Chef",     Bio = "Award-winning desserts",        Specialty = "Tibetan",   IsActive = true, SortOrder = 3 }
        });

        data.Add(new List<Chef>
        {
            new() { Name = "Single Chef",   Title = "Executive Chef",  Bio = "Solo kitchen",                  Specialty = "Fusion",    IsActive = true, SortOrder = 0 }
        });

        data.Add(new List<Chef>
        {
            new() { Name = "Chef A", Title = "Head Chef",  Bio = "Bio A", Specialty = "Himalayan", IsActive = true, SortOrder = 10, ImageUrl = null },
            new() { Name = "Chef B", Title = "Sous Chef",  Bio = "Bio B", Specialty = "Nepalese",  IsActive = true, SortOrder = 20, ImageUrl = "https://example.com/b.jpg" }
        });

        return data;
    }

    public static TheoryData<List<Chef>> GetMixedActiveInactiveChefDatasets()
    {
        var data = new TheoryData<List<Chef>>();

        data.Add(new List<Chef>
        {
            new() { Name = "Active A",   Title = "Head Chef",  Bio = "Bio A", Specialty = "Himalayan", IsActive = true,  SortOrder = 1 },
            new() { Name = "Inactive B", Title = "Sous Chef",  Bio = "Bio B", Specialty = "Nepalese",  IsActive = false, SortOrder = 2 },
            new() { Name = "Active C",   Title = "Pastry",     Bio = "Bio C", Specialty = "Tibetan",   IsActive = true,  SortOrder = 3 }
        });

        data.Add(new List<Chef>
        {
            new() { Name = "All Inactive", Title = "Head Chef", Bio = "Retired", Specialty = "Himalayan", IsActive = false, SortOrder = 1 }
        });

        data.Add(new List<Chef>
        {
            new() { Name = "Only Active", Title = "Chef", Bio = "Active", Specialty = "Nepalese", IsActive = true, SortOrder = 5 }
        });

        return data;
    }

    #endregion
}
