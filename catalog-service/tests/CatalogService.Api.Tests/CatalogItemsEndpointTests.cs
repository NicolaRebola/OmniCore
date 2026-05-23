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
}