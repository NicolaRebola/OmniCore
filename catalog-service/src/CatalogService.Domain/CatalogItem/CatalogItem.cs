namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogItem
{
  public Guid Id { get; }
  public string Name { get; }
  public string Description { get; }
  public string Type { get; }
  public string Visibility { get; }
  public string Status { get; }
  public Guid TenantId { get; }

  public CatalogItem(Guid id, string name, string description, string type, string visibility, string status, Guid tenantId)
  {
    Id = id;
    Name = name;
    Description = description;
    Type = type;
    Visibility = visibility;
    Status = status;
    TenantId = tenantId;
  }
}