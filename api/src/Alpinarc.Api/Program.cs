using Alpinarc.Api;
using Alpinarc.Api.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration);
// Key validation errors by JSON property name ("name", not "Name"), as the web forms expect.
builder.Services.AddControllers(options =>
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()));
builder.Services.AddOpenApi();
// Error responses (unhandled exceptions, unknown routes) use the ProblemDetails shape
// that the OpenAPI document promises, instead of an empty body.
builder.Services.AddProblemDetails();
builder.Services.AddApiHealthChecks();
builder.Services.AddRailwayForwardedHeaders();
builder.Services.AddWebCors();
builder.Services.AddApiRateLimiting(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseWebCors();
app.UseRateLimiter();

app.MapApiHealthChecks();
app.MapControllers();

app.Run();
