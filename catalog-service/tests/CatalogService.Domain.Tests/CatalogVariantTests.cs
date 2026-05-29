using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common;
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
            CatalogVariant.Create(Guid.Empty, Guid.NewGuid(), Status.Active, "XL", "", Guid.NewGuid(), null, null));

        Assert.Equal(DomainErrors.CatalogVariantIdRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyCatalogItemId_ShouldThrow()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogVariant.Create(Guid.NewGuid(), Guid.Empty, Status.Active, "XL", "", Guid.NewGuid(), null, null));

        Assert.Equal(DomainErrors.CatalogItemIdRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogVariant.Create(Guid.NewGuid(), Guid.NewGuid(), Status.Active, "   ", "", Guid.NewGuid(), null, null));

        Assert.Equal(DomainErrors.CatalogItemNameRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateVariant()
    {
        var id = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();

        var variant = CatalogVariant.Create(
            id,
            catalogItemId,
            Status.Active,
            "XL",
            "Extra large size",
            Guid.NewGuid(),
            null,
            null);

        Assert.Equal(id, variant.Id);
        Assert.Equal(catalogItemId, variant.CatalogItemId);
        Assert.Equal("XL", variant.Name);
        Assert.Equal("Extra large size", variant.Description);
        Assert.Equal(Status.Active.Value, variant.Status.Value);
    }

    [Fact]
    public void Create_WithWhitespaceAroundNameAndDescription_ShouldTrimValues()
    {
        var variant = CatalogVariant.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Active,
            "  XL  ",
            "  Extra large size  ",
            Guid.NewGuid(),
            null,
            null);

        Assert.Equal("XL", variant.Name);
        Assert.Equal("Extra large size", variant.Description);
    }

    [Fact]
    public void Create_WithCategoryId_ShouldAssignCategoryId()
    {
        var categoryId = Guid.NewGuid();

        var variant = CatalogVariant.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Active,
            "XL",
            "Extra large size",
            Guid.NewGuid(),
            categoryId,
            null);

        Assert.Equal(categoryId, variant.CategoryId);
    }

    [Fact]
    public void Create_WithPrice_ShouldAssignPrice()
    {
        var price = Price.Create(12.5m, "ars");

        var variant = CatalogVariant.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Active,
            "XL",
            "Extra large size",
            Guid.NewGuid(),
            null,
            price);

        Assert.NotNull(variant.Price);
        Assert.Equal(12.5m, variant.Price.Amount);
        Assert.Equal("ARS", variant.Price.Currency);
    }

    [Fact]
    public void Price_WithNegativeAmount_ShouldThrow()
    {
        var ex = Assert.Throws<CatalogDomainException>(() => Price.Create(-1m, "ARS"));

        Assert.Equal(DomainErrors.InvalidPrice.Code, ex.ErrorCode);
    }
}