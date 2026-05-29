using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using Xunit;

namespace CatalogService.Domain.Tests;

public sealed class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateActiveCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var category = Category.Create(id, "Burgers", Status.Active, tenantId);

        // Assert
        Assert.Equal(id, category.Id);
        Assert.Equal(tenantId, category.TenantId);
        Assert.Equal("Burgers", category.Name);
        Assert.Equal(Status.Active.Value, category.Status.Value);
    }

    [Fact]
    public void Create_WithWhitespaceAroundName_ShouldTrimName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var category = Category.Create(Guid.NewGuid(), "  Burgers  ", Status.Active, tenantId);

        // Assert
        Assert.Equal("Burgers", category.Name);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var ex = Assert.Throws<CatalogDomainException>(() =>
            Category.Create(Guid.NewGuid(), "   ", Status.Active, tenantId));

        // Assert
        Assert.Equal(DomainErrors.CategoryNameRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyTenant_ShouldThrowCatalogDomainException()
    {
        // Act
        var ex = Assert.Throws<CatalogDomainException>(() =>
            Category.Create(Guid.NewGuid(), "Burgers", Status.Active, Guid.Empty));

        // Assert
        Assert.Equal(DomainErrors.CategoryTenantRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Rename_WithValidName_ShouldTrimAndUpdateName()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, Guid.NewGuid());

        // Act
        category.Rename("  Pizza  ");

        // Assert
        Assert.Equal("Pizza", category.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, Guid.NewGuid());

        // Act
        var ex = Assert.Throws<CatalogDomainException>(() => category.Rename("   "));

        // Assert
        Assert.Equal(DomainErrors.CategoryNameRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void ChangeStatus_WithInactiveStatus_ShouldUpdateStatus()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, Guid.NewGuid());

        // Act
        category.ChangeStatus(Status.Inactive);

        // Assert
        Assert.Equal(Status.Inactive.Value, category.Status.Value);
    }
}
