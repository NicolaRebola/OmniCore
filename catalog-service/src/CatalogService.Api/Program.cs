using CatalogService.Api.Errors;
using CatalogService.Application;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(errorApp => 
{
  errorApp.Run(async ctx => 
  {
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
    var exception = feature?.Error;

    var result = exception switch
    {
      CatalogDomainException domainException => ProblemDetailsFactory.Create(ctx, domainException.Error),
      _ => ProblemDetailsFactory.Create(ctx, CatalogErrors.Unexpected)
    };

    await result.ExecuteResultAsync(new ActionContext{HttpContext = ctx});
  });
});

app.MapControllers();


app.Run();