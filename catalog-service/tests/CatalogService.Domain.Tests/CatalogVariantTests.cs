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
            "Extra large size");

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
            "  Extra large size  ");

        Assert.Equal("XL", variant.Name);
        Assert.Equal("Extra large size", variant.Description);
    }
}