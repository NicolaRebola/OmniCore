using CatalogService.Application.Errors;
using CatalogService.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Errors;
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

    public static ObjectResult Create(HttpContext httpContext, ApplicationError error)
    {
        var statusCode = ResolveStatusCode(error);

        var problem = new ProblemDetails
        {
            Type = $"https://docs.omnicore.local/problems/catalog/application/{error.Code.ToLowerInvariant()}",
            Title = error.Title,
            Status = statusCode,
            Detail = error.Detail,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["errorCode"] = error.Code;
        problem.Extensions["layer"] = error.Layer;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static int ResolveStatusCode(ApplicationError error) =>
        error.Code switch
        {
            "CAT-APP-001" => StatusCodes.Status404NotFound,
            "CAT-APP-002" => StatusCodes.Status409Conflict,
            "CAT-APP-004" => StatusCodes.Status404NotFound,
            "CAT-APP-008" => StatusCodes.Status404NotFound,
            "CAT-APP-010" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
}