using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface ICreateCategoryUseCase
{
  Task<CategoryDto> ExecuteAsync(Guid tenantId, CreateCategoryCommand categoryCommand, CancellationToken ct);
}