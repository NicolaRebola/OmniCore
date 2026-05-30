using CatalogService.Api.Contracts;
using CatalogService.Api.Errors;
using CatalogService.Api.Filters;
using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
  private readonly IGetCategoriesUseCase _getCategoriesUseCase;
  private readonly ICreateCategoryUseCase _createCategoryUseCase;
  private readonly IUpdateCategoryUseCase _updateCategoryUseCase;
  public CategoriesController(
    IGetCategoriesUseCase getCategoriesUseCase,
    ICreateCategoryUseCase createCategoryUseCase,
    IUpdateCategoryUseCase updateCategoryUseCase
  )
  {
    _getCategoriesUseCase = getCategoriesUseCase;
    _createCategoryUseCase = createCategoryUseCase;
    _updateCategoryUseCase = updateCategoryUseCase;
  }

  [HttpGet]
  [TenantRequired]
  public async Task<IActionResult> GetAll(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    CancellationToken ct)
  {
    var result = await _getCategoriesUseCase.ExecuteAsync(tenantId, ct);
    return Ok(result);
  }

  [HttpPost]
  [TenantRequired]
  public async Task<IActionResult> Create(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromBody] CreateCategoryCommand command,
    CancellationToken ct)
  {
    if (command == null) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.CreateCategoryCommandInvalid);
    var result = await _createCategoryUseCase.ExecuteAsync(tenantId, command, ct);
    return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
  }

  [HttpPatch("{id:guid}")]
  [TenantRequired]
  public async Task<IActionResult> Patch(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid id,
    [FromBody] UpdateCategoryCommand command,
    CancellationToken ct)
  {
    if (command == null) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.UpdateCategoryCommandInvalid);
    if (command.Name == null && command.Status == null) return NoContent();
    var result = await _updateCategoryUseCase.ExecuteAsync(tenantId, id, command, ct);
    return Ok(result);
  }

  [HttpDelete("{id:guid}")]
  [TenantRequired]
  public async Task<IActionResult> Delete(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    [FromRoute] Guid id,
    CancellationToken ct)
  {
    if (id == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);
    UpdateCategoryCommand command = new(null, "inactive");
    await _updateCategoryUseCase.ExecuteAsync(tenantId, id, command, ct);
    return NoContent();
  }
}