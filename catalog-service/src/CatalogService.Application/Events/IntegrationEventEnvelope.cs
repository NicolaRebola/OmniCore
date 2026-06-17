namespace CatalogService.Application.Events;

public sealed record IntegrationEventEnvelope(
    string SpecVersion,
    Guid EventId,
    string EventType,
    string Source,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid? CorrelationId,
    Guid? CausationId,
    object Data);
