namespace CatalogService.Application.Events;

public static class IntegrationEventEnvelopeBuilder
{
    public const string SpecVersion = "1.0";
    public const string Source = "catalog-service";

    public static IntegrationEventEnvelope Build(
        CatalogIntegrationEvent integrationEvent,
        DateTimeOffset? occurredAt = null,
        Guid? eventId = null)
    {
        return new IntegrationEventEnvelope(
            SpecVersion,
            eventId ?? Guid.NewGuid(),
            integrationEvent.EventType,
            Source,
            occurredAt ?? DateTimeOffset.UtcNow,
            integrationEvent.TenantId,
            integrationEvent.CorrelationId,
            integrationEvent.CausationId,
            integrationEvent.Data);
    }
}
