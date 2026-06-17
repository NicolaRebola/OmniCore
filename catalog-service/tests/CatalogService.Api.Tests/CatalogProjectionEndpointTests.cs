using System.Net;
using System.Text;
using System.Text.Json;
using CatalogService.Infrastructure.Dev;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogService.Api.Tests;

public sealed class CatalogProjectionEndpointTests
{
    private readonly string _seedTenantId = DevSeed.TenantId.ToString();

    private static HttpClient CreateClient()
    {
        return new CatalogWebApplicationFactory().CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetCatalogProjection_WithoutTenantHeader_ShouldReturnTenantRequiredProblem()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/projections/catalog");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal("CAT-API-001", json.RootElement.GetProperty("errorCode").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogProjection_WithEmptyCategoryFilter_ShouldReturnInvalidIdProblem()
    {
        var client = CreateClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/projections/catalog?categoryId=00000000-0000-0000-0000-000000000000");
        request.Headers.Add("X-Tenant-Id", _seedTenantId);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal("CAT-API-003", json.RootElement.GetProperty("errorCode").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogProjection_WithSeedTenant_ShouldReturnUncategorizedActiveCommercialItems()
    {
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/projections/catalog");
        request.Headers.Add("X-Tenant-Id", _seedTenantId);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(_seedTenantId, json.RootElement.GetProperty("tenantId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("createdAt").GetString()));

        var categories = json.RootElement.GetProperty("categories");
        var uncategorized = categories.EnumerateArray().Single(x => x.GetProperty("isVirtual").GetBoolean());
        Assert.Equal("uncategorized", uncategorized.GetProperty("key").GetString());
        Assert.True(uncategorized.GetProperty("items").GetArrayLength() >= 2);

        var item = uncategorized.GetProperty("items")[0];
        Assert.Equal("active", item.GetProperty("status").GetString());
        Assert.Equal("commercial", item.GetProperty("visibility").GetString());
        Assert.True(item.GetProperty("attributes").TryGetProperty("spicy", out var spicy));
        Assert.Equal(JsonValueKind.False, spicy.ValueKind);

        client.Dispose();
    }

    [Fact]
    public async Task GetCatalogProjection_WithCategoryFilter_ShouldReturnOnlyMatchingCategory()
    {
        var client = CreateClient();
        var tenantId = Guid.NewGuid().ToString();
        var categoryId = await CreateCategoryAsync(client, tenantId);
        await CreateCatalogItemAsync(client, tenantId, categoryId);
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/projections/catalog?categoryId={categoryId}");
        request.Headers.Add("X-Tenant-Id", tenantId);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var category = Assert.Single(json.RootElement.GetProperty("categories").EnumerateArray());
        Assert.Equal(categoryId, category.GetProperty("categoryId").GetString());
        Assert.False(category.GetProperty("isVirtual").GetBoolean());
        var item = Assert.Single(category.GetProperty("items").EnumerateArray());
        Assert.Equal(categoryId, item.GetProperty("categoryId").GetString());

        client.Dispose();
    }

    private static async Task<string> CreateCategoryAsync(HttpClient client, string tenantId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/categories");
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Content = JsonContent("""{ "name": "Burgers" }""");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("id").GetString()!;
    }

    private static async Task CreateCatalogItemAsync(HttpClient client, string tenantId, string categoryId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/catalog-items");
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Content = JsonContent($$"""
            {
              "name":"Burger",
              "description":"Classic burger",
              "type":"simple",
              "visibility":"commercial",
              "status":"active",
              "templateId":"bbbbbbbb-0000-0000-0000-000000000001",
              "categoryId":"{{categoryId}}"
            }
            """);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static StringContent JsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");
}
