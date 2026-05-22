using CatalogService.Api.Errors;
using CatalogService.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsFactory
{
    public static ObjectResult Create(HttpContext httpContext, CatalogError error)
    {
        var problem = new ProblemDetails
        {
            Type = error.Type,
            Title = error.Title,
            Status = error.StatusCode,
            Detail = error.Detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["errorCode"] = error.Code;
        problem.Extensions["layer"] = error.Layer;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = error.StatusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static ObjectResult Create(HttpContext httpContext, DomainError error)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://docs.omnicore.local/problems/catalog/domain/{error.Code.ToLowerInvariant()}",
            Title = error.Title,
            Status = StatusCodes.Status400BadRequest, 
            Detail = error.Detail,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["errorCode"] = error.Code;
        problem.Extensions["layer"] = error.Layer;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }
}