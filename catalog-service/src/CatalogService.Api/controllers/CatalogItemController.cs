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
  public async Task<IActionResult> GetAll([FromQuery] Guid tenantId, CancellationToken ct)
  {
      var result = await _useCase.ExecuteAsync(tenantId, ct);
      return Ok(result);
  }
}