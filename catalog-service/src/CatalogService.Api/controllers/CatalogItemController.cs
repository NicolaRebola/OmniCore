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
  public CatalogItemsController(
    IGetCatalogItemsUseCase getCatalogItemsUseCase,
    IGetCatalogItemDetailUseCase getCatalogItemDetailUseCase,
    ICreateCatalogItemUseCase createCatalogItemUseCase)
  {
      _getCatalogItemsUseCase = getCatalogItemsUseCase;
      _getCatalogItemDetailUseCase = getCatalogItemDetailUseCase;
      _createCatalogItemUseCase = createCatalogItemUseCase;
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
}