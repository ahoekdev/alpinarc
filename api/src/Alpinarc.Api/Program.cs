using Alpinarc.Api.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

const string WebCorsPolicy = "Web";
const string ReadyTag = "ready";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AlpinarcDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Alpinarc"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AlpinarcDbContext>(tags: [ReadyTag]);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(WebCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(WebCorsPolicy);
app.UseAuthorization();

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
