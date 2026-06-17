using System.Net;
using System.Text.Json;
using CatalogService.Infrastructure.Dev;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogService.Api.Tests;

public sealed class CatalogTemplatesEndpointTests
{
    private static HttpClient CreateClient()
    {
        return new CatalogWebApplicationFactory().CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetCatalogTemplates_WithoutTenantHeader_ShouldReturnOkAndAllTemplates()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/catalog-templates");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var items = json.RootElement.GetProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.Equal(3, items.GetArrayLength());
        Assert.All(items.EnumerateArray(), item => Assert.False(item.TryGetProperty("tenantId", out _)));
        Assert.Contains(items.EnumerateArray(), item => item.GetProperty("status").GetString() == "inactive");

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogTemplateById_WithExistingTemplate_ShouldReturnOkAndTemplate()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/v1/catalog-templates/{DevSeed.RestaurantCatalogTemplateId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(DevSeed.RestaurantCatalogTemplateId.ToString(), json.RootElement.GetProperty("id").GetString());
        Assert.Equal("Restaurant Item", json.RootElement.GetProperty("name").GetString());
        Assert.Equal("active", json.RootElement.GetProperty("status").GetString());
        Assert.False(json.RootElement.TryGetProperty("tenantId", out _));
        var attributes = json.RootElement.GetProperty("attributes");
        Assert.True(attributes.GetArrayLength() >= 1);
        Assert.Contains(attributes.EnumerateArray(), attribute =>
            attribute.GetProperty("key").GetString() == "serving-size"
            && attribute.GetProperty("type").GetString() == "select"
            && attribute.GetProperty("required").GetBoolean());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogTemplateById_WithInactiveTemplate_ShouldReturnOkAndTemplate()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/v1/catalog-templates/{DevSeed.LegacyCatalogTemplateId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("inactive", json.RootElement.GetProperty("status").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogTemplateById_WithMissingTemplate_ShouldReturnNotFoundProblem()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/v1/catalog-templates/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-APP-010", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Application", json.RootElement.GetProperty("layer").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogTemplateById_WithEmptyTemplateId_ShouldReturnInvalidIdProblem()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/v1/catalog-templates/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-API-003", json.RootElement.GetProperty("errorCode").GetString());

        client.Dispose();
    }
}
