using CatalogService.Api.Contracts;
using CatalogService.Api.Errors;
using CatalogService.Api.Filters;
using CatalogService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/catalog-items")]
public class CatalogItemsController : ControllerBase
{
  private readonly IGetCatalogItemsUseCase _getCatalogItemsUseCase;
  private readonly IGetCatalogItemDetailUseCase _getCatalogItemDetailUseCase;
  public CatalogItemsController(IGetCatalogItemsUseCase getCatalogItemsUseCase, IGetCatalogItemDetailUseCase getCatalogItemDetailUseCase)
  {
      _getCatalogItemsUseCase = getCatalogItemsUseCase;
      _getCatalogItemDetailUseCase = getCatalogItemDetailUseCase;
  }
  [HttpGet]
  [TenantRequired]
  public async Task<IActionResult> GetAll(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    CancellationToken ct)
  {
    var result = await _getCatalogItemsUseCase.ExecuteAsync(tenantId, ct);
    return Ok(result);
  }
  
  [HttpGet("{id:guid}")]
  [TenantRequired]
  public async Task<IActionResult> GetById(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid id,
    CancellationToken ct)
  {
    if (id == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.CatalogItemIdInvalid);
    var result = await _getCatalogItemDetailUseCase.ExecuteAsync(tenantId, id, ct);
    return Ok(result);
  }
}