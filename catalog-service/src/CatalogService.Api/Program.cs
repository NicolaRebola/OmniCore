using CatalogService.Api.Errors;
using CatalogService.Application;
using CatalogService.Infrastructure;
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
app.MapControllers();

app.UseExceptionHandler(errorApp => 
{
  errorApp.Run(async ctx => 
  {
    var result = ProblemDetailsFactory.Create(ctx, CatalogErrors.Unexpected);

    await result.ExecuteResultAsync(new ActionContext{HttpContext = ctx});
  });
});

app.Run();