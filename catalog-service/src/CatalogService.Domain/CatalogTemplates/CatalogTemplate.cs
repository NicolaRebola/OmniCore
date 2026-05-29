using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogTemplates;

public sealed class CatalogTemplate
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Status Status { get; private set; }

    private CatalogTemplate(Guid id, string name, string description, Status status)
    {
        Id = id;
        Name = name;
        Description = description;
        Status = status;
    }

    public static CatalogTemplate Create(Guid id, string name, string description, Status status)
    {
        if (id == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogTemplateIdRequired);
        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogTemplateNameRequired);
        if (status is null) throw new CatalogDomainException(DomainErrors.InvalidStatus);

        return new CatalogTemplate(id, name.Trim(), description?.Trim() ?? string.Empty, status);
    }
}
