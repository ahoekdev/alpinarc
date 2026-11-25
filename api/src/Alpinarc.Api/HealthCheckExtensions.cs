using Alpinarc.Api.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Alpinarc.Api;

public static class HealthCheckExtensions
{
    private const string ReadyTag = "ready";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiHealthChecks()
        {
            services.AddHealthChecks()
                .AddDbContextCheck<AlpinarcDbContext>(tags: [ReadyTag]);
            return services;
        }
    }

    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapApiHealthChecks()
        {
            // Liveness: is the process running? Runs no checks, so a database outage doesn't
            // make the platform restart the API. /health is kept as an alias for existing callers.
            var liveness = new HealthCheckOptions { Predicate = _ => false };
            endpoints.MapHealthChecks("/health/live", liveness);
            endpoints.MapHealthChecks("/health", liveness);

            // Readiness: can it serve traffic? Includes the database check.
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(ReadyTag),
            });
            return endpoints;
        }
    }
}
