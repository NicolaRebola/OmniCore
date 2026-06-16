using CatalogService.Application.Events;

namespace CatalogService.Application.Ports.Outbound;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(CatalogIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    Task PublishAsync(
        IReadOnlyList<CatalogIntegrationEvent> integrationEvents,
        CancellationToken cancellationToken = default);
}
