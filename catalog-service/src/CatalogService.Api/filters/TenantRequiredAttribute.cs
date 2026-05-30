using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogService.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class TenantRequiredAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        => new TenantRequiredFilter();
}