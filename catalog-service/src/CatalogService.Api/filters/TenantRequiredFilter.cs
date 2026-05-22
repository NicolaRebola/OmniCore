using CatalogService.Api.Contracts;
using CatalogService.Api.Errors;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogService.Api.Filters;

public sealed class TenantRequiredFilter : IAsyncResourceFilter
{

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var httpCtx = context.HttpContext;
        var request = httpCtx.Request;

        if (!request.Headers.TryGetValue(TenantHeaders.TenantId, out var headerValues)
            || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
        {
            context.Result = ProblemDetailsFactory.Create(context.HttpContext, CatalogErrors.TenantRequired);
            return;
        }

        if (!Guid.TryParse(headerValues.First(), out var tenantId)) 
        {
            context.Result = ProblemDetailsFactory.Create(context.HttpContext, CatalogErrors.TenantInvalid);
            return;
        }
        
        if (tenantId == Guid.Empty) 
        {
            context.Result = ProblemDetailsFactory.Create(context.HttpContext, CatalogErrors.TenantRequired);
            return;
        }

        await next();
    }
}