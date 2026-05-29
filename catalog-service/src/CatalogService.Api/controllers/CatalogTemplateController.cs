using CatalogService.Api.Errors;
using CatalogService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/catalog-templates")]
public class CatalogTemplatesController : ControllerBase
{
    private readonly IGetCatalogTemplatesUseCase _getCatalogTemplatesUseCase;
    private readonly IGetCatalogTemplateDetailUseCase _getCatalogTemplateDetailUseCase;

    public CatalogTemplatesController(
        IGetCatalogTemplatesUseCase getCatalogTemplatesUseCase,
        IGetCatalogTemplateDetailUseCase getCatalogTemplateDetailUseCase)
    {
        _getCatalogTemplatesUseCase = getCatalogTemplatesUseCase;
        _getCatalogTemplateDetailUseCase = getCatalogTemplateDetailUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getCatalogTemplatesUseCase.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (id == Guid.Empty) return CatalogService.Api.Errors.ProblemDetailsFactory.Create(HttpContext, CatalogErrors.InvalidId);

        var result = await _getCatalogTemplateDetailUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }
}
