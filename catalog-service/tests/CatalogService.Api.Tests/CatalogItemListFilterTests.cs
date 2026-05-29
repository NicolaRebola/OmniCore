using CatalogService.Api.Contracts;
using Xunit;

namespace CatalogService.Api.Tests;

public sealed class CatalogItemListFilterTests
{
  [Fact]
  public void HasValidCategoryFilter_WhenCategoryIdIsMissing_ShouldReturnTrue()
  {
    var filter = new CatalogItemListFilter
    {
      CategoryId = null
    };

    Assert.True(filter.HasValidCategoryFilter());
  }

  [Fact]
  public void HasValidCategoryFilter_WhenCategoryIdIsValidGuid_ShouldReturnTrue()
  {
    var filter = new CatalogItemListFilter
    {
      CategoryId = Guid.NewGuid()
    };

    Assert.True(filter.HasValidCategoryFilter());
  }

  [Fact]
  public void HasValidCategoryFilter_WhenCategoryIdIsEmptyGuid_ShouldReturnFalse()
  {
    var filter = new CatalogItemListFilter
    {
      CategoryId = Guid.Empty
    };

    Assert.False(filter.HasValidCategoryFilter());
  }

  [Fact]
  public void CatalogItemListFilter_ShouldInheritPaginationValidation()
  {
    var filter = new CatalogItemListFilter
    {
      Page = 1,
      PageSize = 20,
      Status = "active"
    };

    Assert.True(filter.HasRequiredPagination());
    Assert.True(filter.HasValidPagination());
  }
}
