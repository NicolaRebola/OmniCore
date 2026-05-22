using CatalogService.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogService.Api.Filters;

public sealed class TenantRequiredFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(TenantHeaders.TenantId, out var headerValues)
            || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault())
            || !Guid.TryParse(headerValues.First(), out var tenantId)
            || tenantId == Guid.Empty)
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "Tenant ID is required",
                Status = StatusCodes.Status400BadRequest,
            })
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentTypes = { "application/problem+json" },
            };

            return;
        }

        await next();
    }
}