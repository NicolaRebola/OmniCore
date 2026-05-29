using CatalogService.Api.Contracts;
using CatalogService.Api.Errors;
using CatalogService.Api.Filters;
using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/catalog-items")]
public class CatalogItemsController : ControllerBase
{
  private readonly IGetCatalogItemsUseCase _getCatalogItemsUseCase;
  private readonly IGetCatalogItemDetailUseCase _getCatalogItemDetailUseCase;
  private readonly ICreateCatalogItemUseCase _createCatalogItemUseCase;
  private readonly IAddCatalogVariantUseCase _addCatalogVariantUseCase;
  private readonly IUpdateCatalogVariantUseCase _updateCatalogVariantUseCase;
  private readonly IDeactivateCatalogVariantUseCase _deactivateCatalogVariantUseCase;
  private readonly IUpdateCatalogItemUseCase _updateCatalogItemUseCase;
  public CatalogItemsController(
    IGetCatalogItemsUseCase getCatalogItemsUseCase,
    IGetCatalogItemDetailUseCase getCatalogItemDetailUseCase,
    ICreateCatalogItemUseCase createCatalogItemUseCase,
    IAddCatalogVariantUseCase addCatalogVariantUseCase,
    IUpdateCatalogVariantUseCase updateCatalogVariantUseCase,
    IDeactivateCatalogVariantUseCase deactivateCatalogVariantUseCase,
    IUpdateCatalogItemUseCase updateCatalogItemUseCase,
    IDeactivateCatalogVariantUseCase deactivateCatalogVariantUseCase)
  {
      _getCatalogItemsUseCase = getCatalogItemsUseCase;
      _getCatalogItemDetailUseCase = getCatalogItemDetailUseCase;
      _createCatalogItemUseCase = createCatalogItemUseCase;
      _addCatalogVariantUseCase = addCatalogVariantUseCase;
      _updateCatalogVariantUseCase = updateCatalogVariantUseCase;
      _deactivateCatalogVariantUseCase = deactivateCatalogVariantUseCase;
      _updateCatalogItemUseCase = updateCatalogItemUseCase;
  }

  [HttpGet]
  [TenantRequired]
  public async Task<IActionResult> GetAll(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromQuery] CatalogItemListFilter filter,
    CancellationToken ct)
  {
    if (!filter.HasRequiredPagination())
    {
      return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.PaginationRequired);
    }

    if (!filter.HasValidPagination())
    {
      return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidPagination);
    }

    if (!filter.HasValidCategoryFilter())
    {
      return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);
    }

    var query = new CatalogItemListQuery(
      tenantId,
      filter.Page!.Value,
      filter.PageSize!.Value,
      filter.Type,
      filter.Visibility,
      filter.Status,
      filter.CategoryId);

    var result = await _getCatalogItemsUseCase.ExecuteAsync(query, ct);
    return Ok(result);
  }
  
  [HttpGet("{id:guid}")]
  [TenantRequired]
  public async Task<IActionResult> GetById(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid id,
    CancellationToken ct)
  {
    if (id == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);
    var result = await _getCatalogItemDetailUseCase.ExecuteAsync(tenantId, id, ct);
    return Ok(result);
  }

  [HttpPost]
  [TenantRequired]
  public async Task<IActionResult> Create(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromBody] CreateCatalogItemCommand command,
    CancellationToken ct) 
  {
    if (command == null) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.CreateCatalogItemInvalid);
    var result = await _createCatalogItemUseCase.ExecuteAsync(tenantId, command, ct);
    return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
  }

  [HttpPost("{itemId:guid}/variants")]
  [TenantRequired]
  public async Task<IActionResult> AddVariant(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid itemId,
    [FromBody] CreateCatalogVariantCommand command,
    CancellationToken ct)
  {
    if (itemId == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);
    if (command == null) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.CreateCatalogItemInvalid);

    var result = await _addCatalogVariantUseCase.ExecuteAsync(tenantId, itemId, command, ct);
    return CreatedAtAction(nameof(GetById), new { id = itemId }, result);
  }

  [HttpPatch("{itemId:guid}/variants/{variantId:guid}")]
  [TenantRequired]
  public async Task<IActionResult> PatchVariant(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid itemId,
    [FromRoute] Guid variantId,
    [FromBody] UpdateCatalogVariantCommand command,
    CancellationToken ct)
  {
    if (itemId == Guid.Empty || variantId == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);
    if (command == null) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.CreateCatalogItemInvalid);

    var result = await _updateCatalogVariantUseCase.ExecuteAsync(tenantId, itemId, variantId, command, ct);
    return Ok(result);
  }

  [HttpDelete("{itemId:guid}/variants/{variantId:guid}")]
  [TenantRequired]
  public async Task<IActionResult> DeleteVariant(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid itemId,
    [FromRoute] Guid variantId,
    CancellationToken ct)
  {
    if (itemId == Guid.Empty || variantId == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);

    await _deactivateCatalogVariantUseCase.ExecuteAsync(tenantId, itemId, variantId, ct);
    return NoContent();
  }

  [HttpPatch("{id:guid}")]
  [TenantRequired]
  public async Task<IActionResult> Update(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid id,
    [FromBody] UpdateCatalogItemCommand command,
    CancellationToken ct)
  {
    if (id == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);
    if (command == null) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.CreateCatalogItemInvalid);
    var result = await _updateCatalogItemUseCase.ExecuteAsync(tenantId, id, command, ct);
    return Ok(result);
  }

}