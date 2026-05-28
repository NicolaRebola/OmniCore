using System.Net;
using System.Text;
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
        Assert.True(json.RootElement.GetArrayLength() >= 3);

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

    [Fact]
    public async Task CreateCatalogItem_WithValidCategoryId_ShouldReturnCreatedItemWithCategory()
    {
        // Arrange
        var client = CreateClient();
        var categoryId = "aaaaaaaa-0000-0000-0000-000000000001";
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/catalog-items");
        request.Headers.Add("X-Tenant-Id", _seedTenantId);
        request.Content = JsonContent($$"""
            {
              "name":"Burger",
              "description":"Classic burger",
              "type":"simple",
              "visibility":"commercial",
              "status":"active",
              "categoryId":"{{categoryId}}"
            }
            """);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(_seedTenantId, json.RootElement.GetProperty("tenantId").GetString());
        Assert.Equal(categoryId, json.RootElement.GetProperty("categoryId").GetString());

        var variant = json.RootElement.GetProperty("variants")[0];
        Assert.Equal(categoryId, variant.GetProperty("categoryId").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task CreateCatalogItem_WithCategoryFromAnotherTenant_ShouldReturnNotFoundProblem()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/catalog-items");
        request.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString());
        request.Content = JsonContent("""
            {
              "name":"Burger",
              "description":"Classic burger",
              "type":"simple",
              "visibility":"commercial",
              "status":"active",
              "categoryId":"aaaaaaaa-0000-0000-0000-000000000001"
            }
            """);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-APP-004", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Application", json.RootElement.GetProperty("layer").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task CreateCatalogItem_WithInactiveCategory_ShouldReturnCategoryNotAssignableProblem()
    {
        // Arrange
        var client = CreateClient();
        var tenantId = Guid.NewGuid().ToString();
        var categoryId = await CreateCategoryAsync(client, tenantId, "Burgers");
        await DeleteCategoryAsync(client, tenantId, categoryId);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/catalog-items");
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Content = JsonContent($$"""
            {
              "name":"Burger",
              "description":"Classic burger",
              "type":"simple",
              "visibility":"commercial",
              "status":"active",
              "categoryId":"{{categoryId}}"
            }
            """);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-APP-007", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Application", json.RootElement.GetProperty("layer").GetString());

        client.Dispose();
    }

    private static StringContent JsonContent(string json)
    {
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<string> CreateCategoryAsync(HttpClient client, string tenantId, string name)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/categories");
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Content = JsonContent($$"""{"name":"{{name}}"}""");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        return json.RootElement.GetProperty("id").GetString()!;
    }

    private static async Task DeleteCategoryAsync(HttpClient client, string tenantId, string categoryId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/categories/{categoryId}");
        request.Headers.Add("X-Tenant-Id", tenantId);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}