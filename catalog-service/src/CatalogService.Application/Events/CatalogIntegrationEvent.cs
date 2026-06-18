namespace CatalogService.Application.Events;

public sealed record CatalogIntegrationEvent(
    string EventType,
    Guid TenantId,
    object Data,
    Guid? CorrelationId = null,
    Guid? CausationId = null);
