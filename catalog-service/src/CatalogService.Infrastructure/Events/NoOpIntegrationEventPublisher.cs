using CatalogService.Application.Events;
using CatalogService.Application.Ports.Outbound;

namespace CatalogService.Infrastructure.Events;

public sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync(CatalogIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return PublishAsync([integrationEvent], cancellationToken);
    }

    public Task PublishAsync(
        IReadOnlyList<CatalogIntegrationEvent> integrationEvents,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
