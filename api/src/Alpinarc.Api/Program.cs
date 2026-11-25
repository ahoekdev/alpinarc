using System.Text.Json.Serialization;
using Alpinarc.Api.Setup;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration);
// Key validation errors by JSON property name ("name", not "Name"), as the web forms expect.
builder.Services.AddControllers(options =>
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()))
    // Numbers must be JSON numbers: the web defaults also accept "42" as a string, which makes
    // the OpenAPI document type every number as integer | string, and the web client with it.
    .AddJsonOptions(options => options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict);
// The OpenAPI document reads these options, not the controllers' ones above.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
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

// No HTTPS redirection: Railway only serves HTTPS at its edge, and an API client that
// follows a redirect has already sent its request over plain HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseWebCors();
app.UseRateLimiter();

app.MapApiHealthChecks();
app.MapControllers();

app.Run();
