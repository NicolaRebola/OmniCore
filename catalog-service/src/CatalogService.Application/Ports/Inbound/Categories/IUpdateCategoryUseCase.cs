using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IUpdateCategoryUseCase
{
  Task<CategoryDto> ExecuteAsync(Guid tenantId, Guid id, UpdateCategoryCommand updateCategoryCommand, CancellationToken ct);
}