using CatalogService.Api.Errors;
using CatalogService.Application;
using CatalogService.Application.Common.Exceptions;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
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
      CatalogApplicationException applicationException => ProblemDetailsFactory.Create(ctx, applicationException.Error),
      _ => ProblemDetailsFactory.Create(ctx, CatalogErrors.Unexpected)
    };

    await result.ExecuteResultAsync(new ActionContext{HttpContext = ctx});
  });
});

app.MapControllers();


app.Run();

public partial class Program { }