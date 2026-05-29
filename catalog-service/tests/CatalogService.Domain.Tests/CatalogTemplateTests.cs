using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using Xunit;

namespace CatalogService.Domain.Tests;

public sealed class CatalogTemplateTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCatalogTemplate()
    {
        var id = Guid.NewGuid();

        var template = CatalogTemplate.Create(id, " Restaurant Item ", " Menu-style products ", Status.Active);

        Assert.Equal(id, template.Id);
        Assert.Equal("Restaurant Item", template.Name);
        Assert.Equal("Menu-style products", template.Description);
        Assert.Equal("active", template.Status.Value);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldThrowCatalogDomainException()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogTemplate.Create(Guid.Empty, "Restaurant Item", "Menu-style products", Status.Active));

        Assert.Equal(DomainErrors.CatalogTemplateIdRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowCatalogDomainException()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogTemplate.Create(Guid.NewGuid(), "   ", "Menu-style products", Status.Active));

        Assert.Equal(DomainErrors.CatalogTemplateNameRequired.Code, ex.ErrorCode);
    }
}
