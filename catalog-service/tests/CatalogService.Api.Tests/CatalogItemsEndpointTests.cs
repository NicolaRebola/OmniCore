using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Api.Tests;

public sealed class CatalogItemsEndpointTests
{
    private readonly string _seedTenantId = DevSeed.TenantId.ToString();

    private static HttpClient CreateClient()
    {
        return new WebApplicationFactory<Program>().CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetCatalogItems_WithSeedTenant_ShouldReturnOkAndItems()
    {
        // Arrange
        var client = CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/catalog-items");
        request.Headers.Add("X-Tenant-Id", _seedTenantId);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Equal(3, json.RootElement.GetArrayLength());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogItems_WithoutTenantHeader_ShouldReturnTenantRequiredProblem()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/catalog-items");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-API-001", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Api", json.RootElement.GetProperty("layer").GetString());
        Assert.Equal("Tenant ID is required", json.RootElement.GetProperty("title").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogItems_WithInvalidTenantHeader_ShouldReturnTenantInvalidProblem()
    {
        // Arrange
        var client = CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/catalog-items");
        request.Headers.Add("X-Tenant-Id", "aaaaaaaa-0000-000");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-API-002", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Api", json.RootElement.GetProperty("layer").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogItems_WithUnknownTenant_ShouldReturnOkAndEmptyArray()
    {
        // Arrange
        var client = CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/catalog-items");
        request.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Equal(0, json.RootElement.GetArrayLength());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogItemDetail_WithSeedItem_ShouldReturnOkAndItemWithVariants()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/catalog-items/aaaaaaaa-0000-0000-0000-000000000001");
        request.Headers.Add("X-Tenant-Id", _seedTenantId);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("aaaaaaaa-0000-0000-0000-000000000001", json.RootElement.GetProperty("id").GetString());
        Assert.Equal("Combo Familiar", json.RootElement.GetProperty("name").GetString());
        Assert.Equal(_seedTenantId, json.RootElement.GetProperty("tenantId").GetString());

        var variants = json.RootElement.GetProperty("variants");
        Assert.Equal(JsonValueKind.Array, variants.ValueKind);
        Assert.NotEqual(0, variants.GetArrayLength());

        var firstVariant = variants[0];
        Assert.False(string.IsNullOrWhiteSpace(firstVariant.GetProperty("id").GetString()));
        Assert.Equal("Combo Familiar", firstVariant.GetProperty("name").GetString());
        Assert.Equal("active", firstVariant.GetProperty("status").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogItemDetail_WithUnknownItem_ShouldReturnNotFoundProblem()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/catalog-items/{Guid.NewGuid()}");
        request.Headers.Add("X-Tenant-Id", _seedTenantId);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-APP-001", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Application", json.RootElement.GetProperty("layer").GetString());
        Assert.Equal("Catalog item not found", json.RootElement.GetProperty("title").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogItemDetail_WithExistingItemFromAnotherTenant_ShouldReturnNotFoundProblem()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/catalog-items/aaaaaaaa-0000-0000-0000-000000000001");
        request.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-APP-001", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Application", json.RootElement.GetProperty("layer").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogItemDetail_WithEmptyItemId_ShouldReturnInvalidItemIdProblem()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/catalog-items/00000000-0000-0000-0000-000000000000");
        request.Headers.Add("X-Tenant-Id", _seedTenantId);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-API-003", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Api", json.RootElement.GetProperty("layer").GetString());

        client.Dispose();
    }

}