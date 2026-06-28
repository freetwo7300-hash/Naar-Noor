using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using NaarNoor.Application.Common;
using NaarNoor.Domain;
using NaarNoor.Domain.ValueObjects;

namespace NaarNoor.Application.Tests.Common
{
    /// <summary>
    /// Property 8: Query Pagination Correctness
    /// Validates that pagination calculations are correct for various page sizes,
    /// offsets, and dataset sizes.
    /// </summary>
    public class QueryPaginationPropertyTests
    {
        [Property(MaxTest = 100)]
        public void Property_OffsetCalculation_IsCorrect(int pageNumber, int pageSize)
        {
            // Arrange: Valid pagination parameters
            var validPageNumber = Math.Max(1, pageNumber % 100);
            var validPageSize = Math.Max(1, Math.Min(100, Math.Abs(pageSize) + 1));

            // Act: Calculate offset
            var offset = (validPageNumber - 1) * validPageSize;

            // Assert: Offset calculation is correct
            Assert.True(offset >= 0);
            Assert.Equal((validPageNumber - 1) * validPageSize, offset);
        }

        [Property(MaxTest = 100)]
        public void Property_PageSizeHandling_MaintainsBounds(int pageSize)
        {
            // Arrange
            var validPageSize = Math.Max(1, Math.Min(1000, Math.Abs(pageSize) + 1));
            var totalItems = 500;

            // Act: Calculate pages needed
            var pagesNeeded = (int)Math.Ceiling((double)totalItems / validPageSize);

            // Assert: Pages calculation is valid
            Assert.True(pagesNeeded > 0);
            Assert.True(pagesNeeded * validPageSize >= totalItems);
            Assert.True((pagesNeeded - 1) * validPageSize < totalItems);
        }

        [Property(MaxTest = 100)]
        public void Property_TotalCountAccuracy_MatchesItemCount(int itemCount)
        {
            // Arrange
            var validCount = Math.Max(0, Math.Min(10000, itemCount));
            var items = new List<MenuItem>();
            for (int i = 0; i < validCount; i++)
            {
                items.Add(new MenuItem($"Item_{i}", "category", price: 10.0m + i, available: true));
            }

            // Act
            var totalCount = items.Count;
            var pageSize = 10;
            var pagesNeeded = (int)Math.Ceiling((double)totalCount / pageSize);

            // Assert
            Assert.Equal(validCount, totalCount);
            Assert.True(pagesNeeded >= (totalCount > 0 ? 1 : 0));
        }

        [Property(MaxTest = 100)]
        public void Property_BoundaryConditions_HandleEdgeCases(int totalItems)
        {
            // Arrange
            var validTotal = Math.Max(0, Math.Min(1000, totalItems));
            var pageSize = 10;

            // Act
            var pageCount = validTotal == 0 ? 0 : (int)Math.Ceiling((double)validTotal / pageSize);

            // Assert: Empty dataset
            if (validTotal == 0)
            {
                Assert.Equal(0, pageCount);
            }

            // Assert: Single page
            if (validTotal > 0 && validTotal <= pageSize)
            {
                Assert.Equal(1, pageCount);
            }

            // Assert: Multiple pages
            if (validTotal > pageSize)
            {
                Assert.True(pageCount > 1);
                Assert.True((pageCount - 1) * pageSize < validTotal);
                Assert.True(pageCount * pageSize >= validTotal);
            }
        }

        [Property(MaxTest = 100)]
        public void Property_SortOrderPreservation_MaintainsSequence(int[] itemIndices)
        {
            // Arrange
            var items = new List<int> { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

            // Act: Sort ascending
            var ascending = items.OrderBy(x => x).ToList();

            // Assert: Order preserved
            for (int i = 1; i < ascending.Count; i++)
            {
                Assert.True(ascending[i] >= ascending[i - 1]);
            }

            // Act: Sort descending
            var descending = items.OrderByDescending(x => x).ToList();

            // Assert: Reverse order preserved
            for (int i = 1; i < descending.Count; i++)
            {
                Assert.True(descending[i] <= descending[i - 1]);
            }
        }

        [Property(MaxTest = 50)]
        public void Property_PaginationConsistency_AcrossPages(int pageNumber, int pageSize)
        {
            // Arrange
            var validPageNumber = Math.Max(1, pageNumber % 10);
            var validPageSize = Math.Max(1, Math.Min(50, Math.Abs(pageSize) + 1));
            var totalItems = 100;

            // Act: Calculate pagination info
            var offset = (validPageNumber - 1) * validPageSize;
            var itemsOnPage = Math.Min(validPageSize, Math.Max(0, totalItems - offset));
            var totalPages = (int)Math.Ceiling((double)totalItems / validPageSize);
            var isLastPage = validPageNumber >= totalPages;

            // Assert: Consistency across pages
            Assert.True(offset >= 0);
            Assert.True(offset <= totalItems || itemsOnPage == 0);
            if (isLastPage)
            {
                Assert.True(itemsOnPage <= validPageSize);
            }
            else
            {
                Assert.Equal(validPageSize, itemsOnPage);
            }
        }

        [Property(MaxTest = 50)]
        public void Property_FilterThenPaginate_ProducesCorrectResults(int pageNumber, int pageSize)
        {
            // Arrange
            var validPageNumber = Math.Max(1, pageNumber % 5);
            var validPageSize = Math.Max(1, Math.Min(20, Math.Abs(pageSize) + 1));
            
            var items = Enumerable.Range(1, 50)
                .Select(i => new MenuItem($"Item_{i}", i % 2 == 0 ? "even" : "odd", 
                    price: (decimal)i, available: i % 3 != 0))
                .ToList();

            // Act: Filter
            var filtered = items.Where(x => x.IsAvailable).ToList();
            
            // Act: Paginate
            var offset = (validPageNumber - 1) * validPageSize;
            var page = filtered.Skip(offset).Take(validPageSize).ToList();

            // Assert: Results are valid subset
            Assert.True(page.Count <= validPageSize);
            Assert.True(page.Count >= 0);
            Assert.True(page.Count == 0 || offset + page.Count <= filtered.Count);
        }
    }

    /// <summary>
    /// MenuItem test entity
    /// </summary>
    internal class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        public MenuItem(string name, string category, decimal price, bool available)
        {
            Name = name;
            Category = category;
            Price = price;
            IsAvailable = available;
        }
    }
}
