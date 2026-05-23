using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using Xunit;

namespace CatalogService.Domain.Tests;

public sealed class CatalogVariantTests
{
    [Fact]
    public void Create_WithEmptyId_ShouldThrow()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogVariant.Create(Guid.Empty, Guid.NewGuid(), Status.Active, "XL", ""));

        Assert.Equal(DomainErrors.CatalogVariantIdRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyCatalogItemId_ShouldThrow()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogVariant.Create(Guid.NewGuid(), Guid.Empty, Status.Active, "XL", ""));

        Assert.Equal(DomainErrors.CatalogItemIdRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogVariant.Create(Guid.NewGuid(), Guid.NewGuid(), Status.Active, "   ", ""));

        Assert.Equal(DomainErrors.CatalogItemNameRequired.Code, ex.ErrorCode);
    }
}