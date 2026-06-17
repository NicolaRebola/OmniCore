using System.Text.Json;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Outbound;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Events;

public sealed class LoggingIntegrationEventPublisher : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<LoggingIntegrationEventPublisher> _logger;

    public LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(CatalogIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return PublishAsync([integrationEvent], cancellationToken);
    }

    public Task PublishAsync(
        IReadOnlyList<CatalogIntegrationEvent> integrationEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var integrationEvent in integrationEvents)
        {
            var envelope = IntegrationEventEnvelopeBuilder.Build(integrationEvent);
            var envelopeJson = JsonSerializer.Serialize(envelope, SerializerOptions);
            _logger.LogInformation(
                "Catalog integration event published: {EventType} ({EventId}) tenant={TenantId} envelope={EnvelopeJson}",
                envelope.EventType,
                envelope.EventId,
                envelope.TenantId,
                envelopeJson);
        }

        return Task.CompletedTask;
    }
}
