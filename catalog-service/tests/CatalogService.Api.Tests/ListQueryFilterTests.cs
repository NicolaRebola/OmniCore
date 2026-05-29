using CatalogService.Api.Contracts;
using Xunit;

namespace CatalogService.Api.Tests;

public sealed class ListQueryFilterTests
{
  [Theory]
  [InlineData(null, 10)]
  [InlineData(1, null)]
  [InlineData(null, null)]
  public void HasRequiredPagination_WhenPageOrPageSizeIsMissing_ShouldReturnFalse(
    int? page,
    int? pageSize)
  {
    var filter = new ListQueryFilter
    {
      Page = page,
      PageSize = pageSize
    };

    Assert.False(filter.HasRequiredPagination());
  }

  [Fact]
  public void HasRequiredPagination_WhenPageAndPageSizeArePresent_ShouldReturnTrue()
  {
    var filter = new ListQueryFilter
    {
      Page = 1,
      PageSize = 20
    };

    Assert.True(filter.HasRequiredPagination());
  }

  [Theory]
  [InlineData(0, 20)]
  [InlineData(1, 0)]
  [InlineData(1, 101)]
  public void HasValidPagination_WhenPaginationIsOutOfRange_ShouldReturnFalse(
    int page,
    int pageSize)
  {
    var filter = new ListQueryFilter
    {
      Page = page,
      PageSize = pageSize
    };

    Assert.False(filter.HasValidPagination());
  }

  [Theory]
  [InlineData(1, 1)]
  [InlineData(1, 100)]
  [InlineData(2, 20)]
  public void HasValidPagination_WhenPaginationIsInRange_ShouldReturnTrue(
    int page,
    int pageSize)
  {
    var filter = new ListQueryFilter
    {
      Page = page,
      PageSize = pageSize
    };

    Assert.True(filter.HasValidPagination());
  }
}
