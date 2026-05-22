using CatalogService.Api.Errors;
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
}