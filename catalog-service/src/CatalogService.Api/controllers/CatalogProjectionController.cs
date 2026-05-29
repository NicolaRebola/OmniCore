using CatalogService.Api.Contracts;
using CatalogService.Api.Errors;
using CatalogService.Api.Filters;
using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/projections/catalog")]
public sealed class CatalogProjectionController : ControllerBase
{
    private readonly IGetCatalogProjectionUseCase _getCatalogProjectionUseCase;

    public CatalogProjectionController(IGetCatalogProjectionUseCase getCatalogProjectionUseCase)
    {
        _getCatalogProjectionUseCase = getCatalogProjectionUseCase;
    }

    [HttpGet]
    [TenantRequired]
    public async Task<IActionResult> Get(
        [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
        [FromQuery] CatalogProjectionFilter filter,
        CancellationToken ct)
    {
        if (!filter.HasValidIds())
        {
            return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);
        }

        var result = await _getCatalogProjectionUseCase.ExecuteAsync(
            new CatalogProjectionQuery(
                tenantId,
                filter.CategoryId,
                filter.ItemType,
                filter.ItemId,
                filter.VariantId),
            ct);

        return Ok(result);
    }
}
