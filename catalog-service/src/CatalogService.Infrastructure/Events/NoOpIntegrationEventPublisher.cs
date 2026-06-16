using CatalogService.Application.Events;
using CatalogService.Application.Ports.Outbound;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Events;

public sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ILogger<NoOpIntegrationEventPublisher>? _logger;

    public NoOpIntegrationEventPublisher(ILogger<NoOpIntegrationEventPublisher>? logger = null)
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
            _logger?.LogDebug(
                "Integration event published: {EventType} ({EventId}) for tenant {TenantId}",
                envelope.EventType,
                envelope.EventId,
                envelope.TenantId);
        }

        return Task.CompletedTask;
    }
}
