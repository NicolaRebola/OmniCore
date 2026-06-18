using CatalogService.Application.Events;
using CatalogService.Application.Events.Payloads;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class IntegrationEventEnvelopeBuilderTests
{
    [Fact]
    public void Build_WithIntegrationEvent_ShouldPopulateRequiredEnvelopeFields()
    {
        var tenantId = Guid.NewGuid();
        var eventId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var occurredAt = new DateTimeOffset(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);
        var integrationEvent = new CatalogIntegrationEvent(
            CatalogEventTypes.VariantPriceChanged,
            tenantId,
            new CatalogVariantPriceChangedPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new PriceEventPayload(10m, "ARS"),
                new PriceEventPayload(12.5m, "ARS")));

        var envelope = IntegrationEventEnvelopeBuilder.Build(integrationEvent, occurredAt, eventId);

        Assert.Equal(IntegrationEventEnvelopeBuilder.SpecVersion, envelope.SpecVersion);
        Assert.Equal(eventId, envelope.EventId);
        Assert.Equal(CatalogEventTypes.VariantPriceChanged, envelope.EventType);
        Assert.Equal(IntegrationEventEnvelopeBuilder.Source, envelope.Source);
        Assert.Equal(occurredAt, envelope.OccurredAt);
        Assert.Equal(tenantId, envelope.TenantId);
        Assert.Null(envelope.CorrelationId);
        Assert.Null(envelope.CausationId);
        Assert.IsType<CatalogVariantPriceChangedPayload>(envelope.Data);
    }
}
