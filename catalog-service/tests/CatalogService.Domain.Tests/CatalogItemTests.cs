using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common;
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
            tenantId,
            null);

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
                tenantId,
                null));

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
                Guid.Empty,
                null));

        // Assert
        Assert.Equal(DomainErrors.CatalogItemTenantRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyTemplateId_ShouldThrowCatalogDomainException()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogItem.CatalogItem.Create(
                Guid.NewGuid(),
                Guid.Empty,
                "Burger",
                "Classic burger",
                CatalogItemType.Simple,
                Visibility.Commercial,
                Status.Active,
                Guid.NewGuid(),
                null));

        Assert.Equal(DomainErrors.CatalogItemTemplateRequired.Code, ex.ErrorCode);
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
            tenantId,
            null);

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
            Guid.NewGuid(),
            null);

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
            Guid.NewGuid(),
            null);

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
            Guid.NewGuid(),
            null);

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
            Guid.NewGuid(),
            null);

        var variant = Assert.Single(item.Variants);

        Assert.NotEqual(Guid.Empty, variant.Id);
        Assert.NotEqual(item.Id, variant.Id);
        Assert.Equal(item.Id, variant.CatalogItemId);
    }

    [Fact]
    public void Create_WithCategoryId_ShouldAssignCategoryToItemAndDefaultVariant()
    {
        var categoryId = Guid.NewGuid();

        var item = CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            Guid.NewGuid(),
            categoryId);

        var variant = Assert.Single(item.Variants);

        Assert.Equal(categoryId, item.CategoryId);
        Assert.Equal(categoryId, variant.CategoryId);
    }

    [Fact]
    public void RenameItem_WithValidName_ShouldUpdateAndTrimName()
    {
        var item = CreateItem();

        item.RenameItem("  Updated Burger  ");

        Assert.Equal("Updated Burger", item.Name);
    }

    [Fact]
    public void ChangeCategory_WithCategoryId_ShouldAssignCategoryToItemAndVariants()
    {
        var item = CreateItem();
        item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);
        var categoryId = Guid.NewGuid();

        item.ChangeCategory(categoryId);

        Assert.Equal(categoryId, item.CategoryId);
        Assert.All(item.Variants, variant => Assert.Equal(categoryId, variant.CategoryId));
    }

    [Fact]
    public void RemoveCategory_WithAssignedCategory_ShouldRemoveCategoryFromItemAndVariants()
    {
        var categoryId = Guid.NewGuid();
        var item = CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            Guid.NewGuid(),
            categoryId);
        item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);

        item.RemoveCategory();

        Assert.Null(item.CategoryId);
        Assert.All(item.Variants, variant => Assert.Null(variant.CategoryId));
    }

    [Fact]
    public void AddVariant_WithValidData_ShouldAppendVariant()
    {
        var item = CreateItem();
        var price = Price.Create(12.5m, "ARS");

        var variant = item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, price);

        Assert.Equal(2, item.Variants.Count);
        Assert.Equal(item.Id, variant.CatalogItemId);
        Assert.Equal("XL", variant.Name);
        Assert.Equal("Extra large", variant.Description);
        Assert.Equal(Status.Active.Value, variant.Status.Value);
        Assert.Equal(price, variant.Price);
    }

    [Fact]
    public void UpdateVariant_WithValidData_ShouldUpdateVariant()
    {
        var item = CreateItem();
        var variantId = item.Variants[0].Id;
        var price = Price.Create(9.99m, "usd");

        var variant = item.UpdateVariant(variantId, "Small", "Small size", Status.Active, price);

        Assert.Equal("Small", variant.Name);
        Assert.Equal("Small size", variant.Description);
        Assert.Equal(Status.Active.Value, variant.Status.Value);
        Assert.Equal(9.99m, variant.Price?.Amount);
        Assert.Equal("USD", variant.Price?.Currency);
    }

    [Fact]
    public void UpdateVariant_WithMultipleActiveVariants_ShouldAllowInactiveStatus()
    {
        var item = CreateItem();
        var variantId = item.Variants[0].Id;
        item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);

        var variant = item.UpdateVariant(variantId, null, null, Status.Inactive, null);

        Assert.Equal(Status.Inactive.Value, variant.Status.Value);
    }

    [Fact]
    public void UpdateVariant_WithLastActiveVariantInactive_ShouldThrowCatalogDomainException()
    {
        var item = CreateItem();
        var variantId = item.Variants[0].Id;

        var ex = Assert.Throws<CatalogDomainException>(() =>
            item.UpdateVariant(variantId, null, null, Status.Inactive, null));

        Assert.Equal(DomainErrors.CatalogItemMustHaveVariant.Code, ex.ErrorCode);
    }

    [Fact]
    public void DeactivateVariant_WithMultipleActiveVariants_ShouldMarkVariantInactive()
    {
        var item = CreateItem();
        var variant = item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);

        var deactivated = item.DeactivateVariant(variant.Id);

        Assert.Equal(Status.Inactive.Value, deactivated.Status.Value);
    }

    [Fact]
    public void DeactivateVariant_WithLastActiveVariant_ShouldThrowCatalogDomainException()
    {
        var item = CreateItem();
        var variantId = item.Variants[0].Id;

        var ex = Assert.Throws<CatalogDomainException>(() => item.DeactivateVariant(variantId));

        Assert.Equal(DomainErrors.CatalogItemMustHaveVariant.Code, ex.ErrorCode);
    }

    [Fact]
    public void UpdateVariant_WithUnknownVariant_ShouldThrowCatalogDomainException()
    {
        var item = CreateItem();

        var ex = Assert.Throws<CatalogDomainException>(() =>
            item.UpdateVariant(Guid.NewGuid(), "Small", null, null, null));

        Assert.Equal(DomainErrors.CatalogVariantNotFound.Code, ex.ErrorCode);
    }

    private static CatalogItem.CatalogItem CreateItem()
    {
        return CatalogItem.CatalogItem.Create(
            Guid.NewGuid(),
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            Guid.NewGuid(),
            null);
    }
}