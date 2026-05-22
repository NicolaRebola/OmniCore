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
  public async Task<IActionResult> GetAll([FromHeader(Name = "X-Tenant-Id")] Guid tenantId, CancellationToken ct)
  {
    if (tenantId == Guid.Empty) return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Validation Error", detail: "Tenant ID is required");
      var result = await _useCase.ExecuteAsync(tenantId, ct);
      return Ok(result);
  }
}