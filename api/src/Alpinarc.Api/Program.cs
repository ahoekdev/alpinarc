using System.Reflection;
using Alpinarc.Api;
using Alpinarc.Api.Data;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

const string WebCorsPolicy = "Web";
const string ReadyTag = "ready";

var builder = WebApplication.CreateBuilder(args);

// Configuration is validated when the app starts, so a misconfigured deploy fails right away
// instead of on the first request. Not while the build generates the OpenAPI document: that
// starts the app too, without any configuration.
var connectionStrings = builder.Services.AddOptions<ConnectionStringsOptions>()
    .BindConfiguration(ConnectionStringsOptions.SectionName);
var webCors = builder.Services.AddOptions<WebCorsOptions>()
    .BindConfiguration(WebCorsOptions.SectionName);
if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    connectionStrings.ValidateDataAnnotations().ValidateOnStart();
    webCors.ValidateDataAnnotations().ValidateOnStart();
}

builder.Services.AddDbContext<AlpinarcDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Alpinarc"))
    .UseSnakeCaseNamingConvention());

// Key validation errors by JSON property name ("name", not "Name"), as the web forms expect.
builder.Services.AddControllers(options =>
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()));
builder.Services.AddOpenApi();
// Error responses (unhandled exceptions, unknown routes) use the ProblemDetails shape
// that the OpenAPI document promises, instead of an empty body.
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AlpinarcDbContext>(tags: [ReadyTag]);

// Railway terminates HTTPS at its edge and forwards plain HTTP. Reading X-Forwarded-Proto
// makes the request count as HTTPS again, so UseHttpsRedirection doesn't redirect it in a
// loop. Railway's proxy addresses aren't fixed, so the known networks and proxies are
// cleared to trust it. The API is only reachable through that proxy.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors();
builder.Services.AddOptions<CorsOptions>()
    .Configure<IOptions<WebCorsOptions>>((options, web) => options.AddPolicy(WebCorsPolicy, policy => policy
        .WithOrigins(web.Value.AllowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// The write endpoints are anonymous until there is authentication, so they share one
// fixed-window limit. It isn't partitioned by client IP, because X-Forwarded-For is
// trusted from any proxy (see above) and so can be spoofed. Rejected requests get a 429,
// which UseStatusCodePages turns into ProblemDetails.
var writesPerMinute = builder.Configuration.GetValue<int>("RateLimiting:WritesPerMinute");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(RateLimitPolicies.Writes, limiter =>
    {
        limiter.PermitLimit = writesPerMinute;
        limiter.Window = TimeSpan.FromMinutes(1);
    });
});

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

app.UseCors(WebCorsPolicy);
app.UseRateLimiter();

// Liveness: is the process running? Runs no checks, so a database outage doesn't
// make the platform restart the API. /health is kept as an alias for existing callers.
var liveness = new HealthCheckOptions { Predicate = _ => false };
app.MapHealthChecks("/health/live", liveness);
app.MapHealthChecks("/health", liveness);

// Readiness: can it serve traffic? Includes the database check.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(ReadyTag),
});
app.MapControllers();

app.Run();
