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

    [Fact]
    public void Create_WithValidData_ShouldCreateDefaultVariantWithOwnIdentity()
    {
        var item = CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            Guid.NewGuid());

        var variant = Assert.Single(item.Variants);

        Assert.NotEqual(Guid.Empty, variant.Id);
        Assert.NotEqual(item.Id, variant.Id);
        Assert.Equal(item.Id, variant.CatalogItemId);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateDescriptiveFields()
    {
        var item = CreateItem();

        item.Update("  Updated Burger  ", "  Better description  ", Visibility.Internal, Status.Inactive);

        Assert.Equal("Updated Burger", item.Name);
        Assert.Equal("Better description", item.Description);
        Assert.Equal(Visibility.Internal.Value, item.Visibility.Value);
        Assert.Equal(Status.Inactive.Value, item.Status.Value);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowCatalogDomainException()
    {
        var item = CreateItem();

        var ex = Assert.Throws<CatalogDomainException>(() =>
            item.Update("   ", "Updated description", Visibility.Internal, Status.Inactive));

        Assert.Equal(DomainErrors.CatalogItemNameRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Update_WithNullDescription_ShouldNormalizeToEmptyDescription()
    {
        var item = CreateItem();

        item.Update("Burger", null, Visibility.Commercial, Status.Active);

        Assert.Equal("", item.Description);
    }

    [Fact]
    public void Update_WithNullVisibility_ShouldThrowCatalogDomainException()
    {
        var item = CreateItem();

        var ex = Assert.Throws<CatalogDomainException>(() =>
            item.Update("Burger", "Description", null, Status.Active));

        Assert.Equal(DomainErrors.InvalidVisibility.Code, ex.ErrorCode);
    }

    [Fact]
    public void Update_WithNullStatus_ShouldThrowCatalogDomainException()
    {
        var item = CreateItem();

        var ex = Assert.Throws<CatalogDomainException>(() =>
            item.Update("Burger", "Description", Visibility.Commercial, null));

        Assert.Equal(DomainErrors.InvalidStatus.Code, ex.ErrorCode);
    }

    [Fact]
    public void Update_ShouldKeepExistingVariantsUnchanged()
    {
        var item = CreateItem();
        var variant = Assert.Single(item.Variants);
        var originalVariantName = variant.Name;
        var originalVariantDescription = variant.Description;
        var originalVariantStatus = variant.Status.Value;

        item.Update("Updated Burger", "Updated description", Visibility.Internal, Status.Inactive);

        var updatedVariant = Assert.Single(item.Variants);
        Assert.Equal(originalVariantName, updatedVariant.Name);
        Assert.Equal(originalVariantDescription, updatedVariant.Description);
        Assert.Equal(originalVariantStatus, updatedVariant.Status.Value);
    }

    private static CatalogItem.CatalogItem CreateItem() =>
        CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            Guid.NewGuid());
}