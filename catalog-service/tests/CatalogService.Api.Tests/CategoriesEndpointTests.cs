using System.Net;
using System.Text;
using System.Text.Json;
using CatalogService.Infrastructure.Dev;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogService.Api.Tests;

public sealed class CategoriesEndpointTests
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
    public async Task GetCategories_WithSeedTenant_ShouldReturnOkAndCategories()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
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

        var firstCategory = json.RootElement[0];
        Assert.False(string.IsNullOrWhiteSpace(firstCategory.GetProperty("id").GetString()));
        Assert.Equal(_seedTenantId, firstCategory.GetProperty("tenantId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(firstCategory.GetProperty("name").GetString()));
        Assert.Equal("active", firstCategory.GetProperty("status").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task GetCategories_WithoutTenantHeader_ShouldReturnTenantRequiredProblem()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/categories");

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
    public async Task GetCategories_WithUnknownTenant_ShouldReturnOkAndEmptyArray()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
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
    public async Task CreateCategory_WithValidRequest_ShouldReturnCreatedAndCategory()
    {
        // Arrange
        var client = CreateClient();
        var tenantId = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/categories");
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Content = JsonContent("""{"name":"Burgers"}""");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("id").GetString()));
        Assert.Equal(tenantId, json.RootElement.GetProperty("tenantId").GetString());
        Assert.Equal("Burgers", json.RootElement.GetProperty("name").GetString());
        Assert.Equal("active", json.RootElement.GetProperty("status").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task CreateCategory_WithoutTenantHeader_ShouldReturnTenantRequiredProblem()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/categories");
        request.Content = JsonContent("""{"name":"Burgers"}""");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-API-001", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Api", json.RootElement.GetProperty("layer").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ShouldReturnDomainProblem()
    {
        // Arrange
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/categories");
        request.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString());
        request.Content = JsonContent("""{"name":"   "}""");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-DOM-009", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Domain", json.RootElement.GetProperty("layer").GetString());
        Assert.Equal("Category name is required", json.RootElement.GetProperty("title").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task PatchCategory_WithValidRequest_ShouldReturnOkAndUpdatedCategory()
    {
        // Arrange
        var client = CreateClient();
        var tenantId = Guid.NewGuid().ToString();
        var categoryId = await CreateCategoryAsync(client, tenantId, "Burgers");

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/categories/{categoryId}");
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Content = JsonContent("""{"name":"Pizza","status":"inactive"}""");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(categoryId, json.RootElement.GetProperty("id").GetString());
        Assert.Equal(tenantId, json.RootElement.GetProperty("tenantId").GetString());
        Assert.Equal("Pizza", json.RootElement.GetProperty("name").GetString());
        Assert.Equal("inactive", json.RootElement.GetProperty("status").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task PatchCategory_WithExistingCategoryFromAnotherTenant_ShouldReturnNotFoundProblem()
    {
        // Arrange
        var client = CreateClient();
        var ownerTenantId = Guid.NewGuid().ToString();
        var otherTenantId = Guid.NewGuid().ToString();
        var categoryId = await CreateCategoryAsync(client, ownerTenantId, "Burgers");

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/categories/{categoryId}");
        request.Headers.Add("X-Tenant-Id", otherTenantId);
        request.Content = JsonContent("""{"name":"Pizza"}""");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("CAT-APP-004", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Application", json.RootElement.GetProperty("layer").GetString());
        Assert.Equal("Category not found", json.RootElement.GetProperty("title").GetString());

        client.Dispose();
    }

    [Fact]
    public async Task DeleteCategory_WithValidRequest_ShouldReturnNoContentAndRemoveFromList()
    {
        // Arrange
        var client = CreateClient();
        var tenantId = Guid.NewGuid().ToString();
        var categoryId = await CreateCategoryAsync(client, tenantId, "Burgers");

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/categories/{categoryId}");
        deleteRequest.Headers.Add("X-Tenant-Id", tenantId);

        // Act
        var deleteResponse = await client.SendAsync(deleteRequest);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
        getRequest.Headers.Add("X-Tenant-Id", tenantId);
        var getResponse = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var body = await getResponse.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Equal(0, json.RootElement.GetArrayLength());

        client.Dispose();
    }

    [Fact]
    public async Task DeleteCategory_WithExistingCategoryFromAnotherTenant_ShouldReturnNotFoundProblem()
    {
        // Arrange
        var client = CreateClient();
        var ownerTenantId = Guid.NewGuid().ToString();
        var otherTenantId = Guid.NewGuid().ToString();
        var categoryId = await CreateCategoryAsync(client, ownerTenantId, "Burgers");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/categories/{categoryId}");
        request.Headers.Add("X-Tenant-Id", otherTenantId);

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
}
