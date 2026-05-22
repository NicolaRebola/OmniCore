using CatalogService.Api.Contracts;
using CatalogService.Api.Filters;
using CatalogService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/catalog-items")]
public class CatalogItemsController : ControllerBase
{
  private readonly IGetCatalogItemsUseCase _useCase;
  public CatalogItemsController(IGetCatalogItemsUseCase useCase)
  {
      _useCase = useCase;
  }
  [HttpGet]
  [TenantRequired]
  public async Task<IActionResult> GetAll(
    [FromHeader(Name = TenantHeaders.TenantId)] Guid tenantId,
    CancellationToken ct)
  {
    try {
      var result = await _useCase.ExecuteAsync(tenantId, ct);
      return Ok(result);
    } catch (Exception ex) {
      return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error", detail: ex.Message);
    }
  }
}