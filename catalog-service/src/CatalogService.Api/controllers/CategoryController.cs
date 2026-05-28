using CatalogService.Api.Contracts;
using CatalogService.Api.Filters;
using CatalogService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
  private readonly IGetCategoriesUseCase _getCategoriesUseCase;

  public CategoriesController(
    IGetCategoriesUseCase getCategoriesUseCase
  )
  {
    _getCategoriesUseCase = getCategoriesUseCase;
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
}