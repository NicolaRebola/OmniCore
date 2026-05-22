using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using Xunit;

namespace CatalogService.Domain.Tests;

public sealed class CatalogItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCatalogItem()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var item = CatalogItem.CatalogItem.Create(
            id,
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            tenantId);

        // Assert
        Assert.Equal(id, item.Id);
        Assert.Equal("Burger", item.Name);
        Assert.Equal("Classic burger", item.Description);
        Assert.Equal(CatalogItemType.Simple.Value, item.Type.Value);
        Assert.Equal(Visibility.Commercial.Value, item.Visibility.Value);
        Assert.Equal(tenantId, item.TenantId);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogItem.CatalogItem.Create(
                id,
                "   ",
                "Classic burger",
                CatalogItemType.Simple,
                Visibility.Commercial,
                Status.Active,
                tenantId));

        // Assert
        Assert.Equal(DomainErrors.CatalogItemNameRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyTenant_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogItem.CatalogItem.Create(
                id,
                "Burger",
                "Classic burger",
                CatalogItemType.Simple,
                Visibility.Commercial,
                Status.Active,
                Guid.Empty));

        // Assert
        Assert.Equal(DomainErrors.CatalogItemTenantRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateDefaultVariant()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var item = CatalogItem.CatalogItem.Create(
            id,
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            tenantId);

        // Assert
        Assert.NotEmpty(item.Variants);
        Assert.Single(item.Variants);
        Assert.Equal(item.Id, item.Variants[0].CatalogItemId);
    }
}