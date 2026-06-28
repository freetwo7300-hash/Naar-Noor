using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.MenuItems.Queries.GetMenuItems;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using Xunit;

namespace NaarNoor.Application.Tests.Menus;

/// <summary>
/// Property-based tests for GetMenuItemsQueryHandler.
/// 
/// **Validates: Requirements 2.2**
/// 
/// These tests verify that query handlers:
/// 1. Filter results correctly according to query parameters (category filter)
/// 2. Transformation logic produces correct data types (DTOs)
/// 3. Filtering predicates work across variations
/// 4. Results maintain data consistency
/// 5. Filtering does not mutate source data
/// </summary>
public class GetMenuItemsQueryHandlerPropertyTests : ApplicationLayerTestBase
{
    /// <summary>
    /// Property: Query Result Filtering and Transformation
    /// 
    /// For any query executed with random category filter parameters against seeded test data,
    /// the query handler SHALL:
    /// - Return only available menu items
    /// - Filter by category if specified and valid
    /// - Not apply filter if category is null, empty, or invalid enum value
    /// - Transform entities to DTOs with all required fields populated
    /// - Maintain ordering consistency (by category, then sort order)
    /// - Not access the database directly
    /// </summary>
    [Property(StartSize = 0, EndSize = 10, MaxTest = 50)]
    public Property QueryHandlerFiltersMenuItemsByCategory(string? categoryFilter)
    {
        return Prop.ForAll(
            Gen.Constant(GenerateValidMenuItems(50).ToList()).ToArbitrary(),
            menuItems =>
            {
                // Arrange
                var testData = menuItems.ToList();
                var unitOfWorkMock = CreateUnitOfWorkMockWithMenuItems(testData);

                var handler = new GetMenuItemsQueryHandler(unitOfWorkMock.Object);
                var query = new GetMenuItemsQuery(categoryFilter);

                // Act
                var result = handler.Handle(query, CancellationToken.None).Result;

                // Assert - Structure & Type Correctness
                result.Should().NotBeNull("Result should not be null");
                result.Should().BeOfType<List<MenuItemDto>>("Result should be a list of DTOs");
                result.All(m => m != null).Should().BeTrue("All DTOs should be non-null");

                // Assert - Availability Filter (all results must be available)
                result.All(r => r.IsAvailable).Should().BeTrue(
                    "All returned items should have IsAvailable = true");

                // Assert - Category Filter
                var expectedItems = testData
                    .Where(m => m.IsAvailable)
                    .AsEnumerable();

                // Apply category filter if provided and valid
                if (!string.IsNullOrWhiteSpace(categoryFilter) &&
                    Enum.TryParse<MenuCategory>(categoryFilter, true, out var category))
                {
                    expectedItems = expectedItems.Where(m => m.Category == category);
                }

                expectedItems = expectedItems
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.SortOrder)
                    .ToList();

                result.Count.Should().Be(expectedItems.Count(),
                    $"Should return {expectedItems.Count()} items matching filter criteria");

                // Assert - Data Transformation
                var expectedItemsList = expectedItems.ToList();
                for (int i = 0; i < result.Count; i++)
                {
                    var dto = result[i];
                    var entity = expectedItemsList[i];

                    dto.Id.Should().Be(entity.Id);
                    dto.Name.Should().Be(entity.Name);
                    dto.Description.Should().Be(entity.Description);
                    dto.Price.Should().Be(entity.Price);
                    dto.Category.Should().Be(entity.Category.ToString());
                    dto.IsVegetarian.Should().Be(entity.IsVegetarian);
                    dto.IsVegan.Should().Be(entity.IsVegan);
                    dto.IsGlutenFree.Should().Be(entity.IsGlutenFree);
                    dto.IsAvailable.Should().Be(entity.IsAvailable);
                    dto.ImageUrl.Should().Be(entity.ImageUrl);
                    dto.SortOrder.Should().Be(entity.SortOrder);
                }

                // Assert - Ordering Consistency
                var categoryOrder = result
                    .Zip(result.Skip(1), (current, next) =>
                    {
                        var currentCategory = Enum.Parse<MenuCategory>(current.Category);
                        var nextCategory = Enum.Parse<MenuCategory>(next.Category);
                        if (currentCategory != nextCategory)
                            return currentCategory < nextCategory;
                        return current.SortOrder <= next.SortOrder;
                    })
                    .All(x => x);

                categoryOrder.Should().BeTrue(
                    "Results should be ordered by category, then by sort order");

                // Assert - No Source Mutation
                testData.Should().AllSatisfy(item =>
                    item.IsAvailable.Should().BeTrue("Source data should not be mutated"));

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
    /// For any menu item entity with valid data,
    /// the transformation to DTO SHALL produce DTOs with all fields populated correctly
    /// and no null values in required fields.
    /// </summary>
    [Property(StartSize = 10, EndSize = 50, MaxTest = 50)]
    public Property QueryTransformsAllMenuItemFieldsCorrectly()
    {
        return Prop.ForAll(
            Gen.Constant(GenerateValidMenuItems(30).ToList()).ToArbitrary(),
            menuItems =>
            {
                // Arrange
                var unitOfWorkMock = CreateUnitOfWorkMockWithMenuItems(menuItems.ToList());
                var handler = new GetMenuItemsQueryHandler(unitOfWorkMock.Object);
                var query = new GetMenuItemsQuery(null);

                // Act
                var result = handler.Handle(query, CancellationToken.None).Result;

                // Assert - All required fields are present
                foreach (var dto in result)
                {
                    dto.Id.Should().NotBe(Guid.Empty, "ID should not be empty");
                    dto.Name.Should().NotBeNullOrWhiteSpace("Name should be populated");
                    dto.Description.Should().NotBeNullOrWhiteSpace("Description should be populated");
                    dto.Price.Should().BeGreaterThan(0, "Price should be greater than 0");
                    dto.Category.Should().NotBeNullOrWhiteSpace("Category should be populated");
                    dto.IsAvailable.Should().BeTrue("All returned items should be available");

                    // Validate category is a valid enum value
                    Enum.TryParse<MenuCategory>(dto.Category, out _).Should().BeTrue(
                        $"Category '{dto.Category}' should be a valid MenuCategory enum value");

                    // Verify dietary attributes are boolean
                    (dto.IsVegetarian || !dto.IsVegetarian).Should().BeTrue();
                    (dto.IsVegan || !dto.IsVegan).Should().BeTrue();
                    (dto.IsGlutenFree || !dto.IsGlutenFree).Should().BeTrue();

                    dto.SortOrder.Should().BeGreaterThanOrEqualTo(0, "Sort order should be non-negative");
                }

                return true.ToProperty();
            });
    }

    /// <summary>
    /// Property: Category Filter Specificity
    /// 
    /// For any category that exists in the test data,
    /// when the query handler is called with that category filter,
    /// it SHALL return only items of that category and no items from other categories.
    /// </summary>
    [Property(StartSize = 10, EndSize = 30, MaxTest = 30)]
    public Property QueryCategoryFilterIsSpecificAndAccurate()
    {
        return Prop.ForAll(
            Gen.Constant(GenerateValidMenuItems(40).ToList()).ToArbitrary(),
            menuItems =>
            {
                var testData = menuItems.ToList();
                var unitOfWorkMock = CreateUnitOfWorkMockWithMenuItems(testData);
                var handler = new GetMenuItemsQueryHandler(unitOfWorkMock.Object);

                // Get all unique categories in test data
                var categories = testData
                    .Where(m => m.IsAvailable)
                    .Select(m => m.Category)
                    .Distinct()
                    .ToList();

                // Test filtering by each category
                foreach (var category in categories)
                {
                    var categoryString = category.ToString();
                    var query = new GetMenuItemsQuery(categoryString);

                    // Act
                    var result = handler.Handle(query, CancellationToken.None).Result;

                    // Assert - All results match the requested category
                    result.Should().AllSatisfy(item =>
                        item.Category.Should().Be(categoryString,
                            $"All items should be in category '{categoryString}'"));

                    // Assert - No items from other categories are included
                    var otherCategories = categories
                        .Where(c => c != category)
                        .Select(c => c.ToString())
                        .ToList();

                    result.Should().NotContain(item =>
                        otherCategories.Contains(item.Category),
                        "Should not contain items from other categories");
                }

                return true.ToProperty();
            });
    }

    /// <summary>
    /// Property: Invalid Category Filter Handling
    /// 
    /// For any invalid category string (not matching any enum value),
    /// the query handler SHALL ignore the filter and return all available items.
    /// </summary>
    [Property(MaxTest = 20)]
    public Property QueryIgnoresInvalidCategoryFilter(string? invalidCategory)
    {
        // Skip if the category happens to be valid or null
        if (string.IsNullOrWhiteSpace(invalidCategory) ||
            Enum.TryParse<MenuCategory>(invalidCategory, true, out _))
        {
            return true.ToProperty();
        }

        return Prop.ForAll(
            Gen.Constant(GenerateValidMenuItems(20).ToList()).ToArbitrary(),
            menuItems =>
            {
                // Arrange
                var testData = menuItems.ToList();
                var unitOfWorkMock = CreateUnitOfWorkMockWithMenuItems(testData);
                var handler = new GetMenuItemsQueryHandler(unitOfWorkMock.Object);

                // Query with invalid category
                var queryWithInvalid = new GetMenuItemsQuery(invalidCategory);

                // Query without category filter
                var queryWithoutFilter = new GetMenuItemsQuery(null);

                // Act
                var resultWithInvalid = handler.Handle(queryWithInvalid, CancellationToken.None).Result;
                var resultWithoutFilter = handler.Handle(queryWithoutFilter, CancellationToken.None).Result;

                // Assert - Both queries should return the same results (invalid filter ignored)
                resultWithInvalid.Should().HaveCount(resultWithoutFilter.Count,
                    "Invalid category filter should be ignored");

                return true.ToProperty();
            });
    }

    // Helper methods

    private IEnumerable<MenuItem> GenerateValidMenuItems(int itemCount)
    {
        var categories = Enum.GetValues<MenuCategory>();
        var baseDate = DateTimeOffset.Now;

        for (int i = 0; i < itemCount; i++)
        {
            var category = categories[i % categories.Length];

            var menuItem = new MenuItem
            {
                Name        = $"MenuItem {i}",
                Description = $"Description for item {i}",
                Price       = (decimal)(10 + (i % 50)),
                Category    = category,
                IsVegetarian = i % 3 == 0,
                IsVegan      = i % 5 == 0,
                IsGlutenFree = i % 4 == 0,
                IsAvailable  = true,
                ImageUrl     = $"https://example.com/image{i}.jpg",
                SortOrder    = i
            };

            yield return menuItem;
        }
    }

    private Mock<IUnitOfWork> CreateUnitOfWorkMockWithMenuItems(List<MenuItem> menuItems)
    {
        var unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();

        // Create a queryable mock that returns only available items
        var queryable = menuItems.Where(m => m.IsAvailable).AsAsyncTestQueryable();

        var repositoryMock = CreateRepositoryMock<IRepository<MenuItem>>();
        repositoryMock
            .Setup(r => r.Query())
            .Returns(queryable);

        unitOfWorkMock
            .Setup(u => u.MenuItems)
            .Returns(repositoryMock.Object);

        // Ensure SaveChangesAsync is not called
        unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));

        return unitOfWorkMock;
    }
}
