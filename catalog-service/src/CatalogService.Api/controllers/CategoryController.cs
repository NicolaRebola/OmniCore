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
  public CategoriesController(
    IGetCategoriesUseCase getCategoriesUseCase,
    ICreateCategoryUseCase createCategoryUseCase
  )
  {
    _getCategoriesUseCase = getCategoriesUseCase;
    _createCategoryUseCase = createCategoryUseCase;
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
}