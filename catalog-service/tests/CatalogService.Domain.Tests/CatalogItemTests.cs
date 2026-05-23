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
        Assert.Equal(item.Id, item.Variants[0].CatalogItemId);
        Assert.Equal(item.Name, item.Variants[0].Name);
        Assert.Equal(item.Description, item.Variants[0].Description);
        Assert.Equal(item.Status.Value, item.Variants[0].Status.Value);
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldStillCreateDefaultVariant()
    {
        var item = CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Burger",
            "",  // descripción vacía permitida en item
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            Guid.NewGuid());

        Assert.Single(item.Variants);
        Assert.Equal("Burger", item.Variants[0].Name);
        Assert.Equal("", item.Variants[0].Description);
    }

    [Fact]
    public void Create_WithInactiveStatus_ShouldCreateDefaultVariantWithSameStatus()
    {
        var item = CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Tea",
            "Herbal",
            CatalogItemType.Simple,
            Visibility.Internal,
            Status.Inactive,
            Guid.NewGuid());

        Assert.Equal(Status.Inactive.Value, item.Variants[0].Status.Value);
    }

    [Fact]
    public void Create_VariableItem_ShouldStillHaveAtLeastOneDefaultVariant()
    {
        var item = CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Combo Familiar",
            "Burger + fries",
            CatalogItemType.Variable,
            Visibility.Commercial,
            Status.Active,
            Guid.NewGuid());

        Assert.Single(item.Variants);
    }
}