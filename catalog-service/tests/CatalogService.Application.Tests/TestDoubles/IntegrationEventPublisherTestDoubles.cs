using CatalogService.Application.Events;
using CatalogService.Application.Ports.Outbound;

namespace CatalogService.Application.Tests.TestDoubles;

public sealed class RecordingIntegrationEventPublisher : IIntegrationEventPublisher
{
    public List<CatalogIntegrationEvent> Published { get; } = [];

    public Task PublishAsync(CatalogIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Published.Add(integrationEvent);
        return Task.CompletedTask;
    }

    public Task PublishAsync(
        IReadOnlyList<CatalogIntegrationEvent> integrationEvents,
        CancellationToken cancellationToken = default)
    {
        Published.AddRange(integrationEvents);
        return Task.CompletedTask;
    }
}

public sealed class NullIntegrationEventPublisher : IIntegrationEventPublisher
{
    public static readonly NullIntegrationEventPublisher Instance = new();

    private NullIntegrationEventPublisher()
    {
    }

    public Task PublishAsync(CatalogIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(
        IReadOnlyList<CatalogIntegrationEvent> integrationEvents,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
