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

    [Fact]
    public void Create_WithAttributeDefinitions_ShouldKeepDefinitions()
    {
        var attribute = AttributeDefinition.Create(
            Guid.NewGuid(),
            "serving-size",
            "Serving Size",
            AttributeType.Select,
            required: true,
            defaultValue: "regular",
            options: ["regular", "large"]);

        var template = CatalogTemplate.Create(Guid.NewGuid(), "Restaurant Item", "Menu-style products", Status.Active, [attribute]);

        var result = Assert.Single(template.Attributes);
        Assert.Equal("serving-size", result.Key);
        Assert.Equal("select", result.Type.Value);
        Assert.True(result.Required);
        Assert.Equal("regular", result.DefaultValue);
        Assert.Equal(["regular", "large"], result.Options);
    }

    [Fact]
    public void CreateAttribute_WithUnsupportedType_ShouldThrowCatalogDomainException()
    {
        var ex = Assert.Throws<CatalogDomainException>(() => AttributeType.From("image"));

        Assert.Equal(DomainErrors.InvalidAttributeType.Code, ex.ErrorCode);
    }

    [Fact]
    public void CreateAttribute_WithSelectWithoutOptions_ShouldThrowCatalogDomainException()
    {
        var ex = Assert.Throws<CatalogDomainException>(() =>
            AttributeDefinition.Create(Guid.NewGuid(), "color", "Color", AttributeType.Select, false, null, []));

        Assert.Equal(DomainErrors.AttributeOptionsRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public void Create_WithDuplicateAttributeKey_ShouldThrowCatalogDomainException()
    {
        var first = AttributeDefinition.Create(Guid.NewGuid(), "color", "Color", AttributeType.Text, false, null, []);
        var second = AttributeDefinition.Create(Guid.NewGuid(), "color", "Color again", AttributeType.Text, false, null, []);

        var ex = Assert.Throws<CatalogDomainException>(() =>
            CatalogTemplate.Create(Guid.NewGuid(), "Restaurant Item", "Menu-style products", Status.Active, [first, second]));

        Assert.Equal(DomainErrors.AttributeDefinitionKeyAlreadyExists.Code, ex.ErrorCode);
    }

    [Fact]
    public void ValidateValues_WithUnknownAttribute_ShouldThrowCatalogDomainException()
    {
        var template = CatalogTemplate.Create(Guid.NewGuid(), "Restaurant Item", "Menu-style products", Status.Active);
        var values = new[] { AttributeValue.Create("unknown", "value") };

        var ex = Assert.Throws<CatalogDomainException>(() => template.ValidateValues(values));

        Assert.Equal(DomainErrors.AttributeDefinitionNotFound.Code, ex.ErrorCode);
    }

    [Fact]
    public void ValidateValues_WithInvalidSelectOption_ShouldThrowCatalogDomainException()
    {
        var color = AttributeDefinition.Create(Guid.NewGuid(), "color", "Color", AttributeType.Select, false, null, ["black", "white"]);
        var template = CatalogTemplate.Create(Guid.NewGuid(), "Retail Product", "Retail products", Status.Active, [color]);
        var values = new[] { AttributeValue.Create("color", "red") };

        var ex = Assert.Throws<CatalogDomainException>(() => template.ValidateValues(values));

        Assert.Equal(DomainErrors.InvalidAttributeValue.Code, ex.ErrorCode);
    }

    [Fact]
    public void EnsureRequiredVariantAttributesAreSatisfied_WithoutValueOrDefault_ShouldThrowCatalogDomainException()
    {
        var size = AttributeDefinition.Create(Guid.NewGuid(), "size", "Size", AttributeType.Select, true, null, ["regular", "large"]);
        var template = CatalogTemplate.Create(Guid.NewGuid(), "Restaurant Item", "Menu-style products", Status.Active, [size]);

        var ex = Assert.Throws<CatalogDomainException>(() => template.EnsureRequiredVariantAttributesAreSatisfied([], []));

        Assert.Equal(DomainErrors.RequiredAttributeValueMissing.Code, ex.ErrorCode);
    }

    [Fact]
    public void Resolve_WithVariantItemAndDefaultValues_ShouldUseExpectedPrecedence()
    {
        var color = AttributeDefinition.Create(Guid.NewGuid(), "color", "Color", AttributeType.Select, false, "red", ["red", "black"]);
        var size = AttributeDefinition.Create(Guid.NewGuid(), "size", "Size", AttributeType.Select, false, "regular", ["regular", "large"]);
        var template = CatalogTemplate.Create(Guid.NewGuid(), "Restaurant Item", "Menu-style products", Status.Active, [color, size]);
        var itemValues = new[] { AttributeValue.Create("color", "black") };
        var variantValues = new[] { AttributeValue.Create("size", "large") };

        var result = AttributeResolver.Resolve(template, itemValues, variantValues);

        Assert.Contains(result, x => x.Key == "color" && x.Value == "black" && x.Source == "item");
        Assert.Contains(result, x => x.Key == "size" && x.Value == "large" && x.Source == "variant");
    }
}
