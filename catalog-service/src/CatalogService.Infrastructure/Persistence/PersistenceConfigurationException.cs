namespace CatalogService.Infrastructure.Persistence;

public sealed class PersistenceConfigurationException : Exception
{
    public PersistenceConfigurationException(string message) : base(message)
    {
    }
}
