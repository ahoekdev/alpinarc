using Microsoft.AspNetCore.RateLimiting;

namespace Alpinarc.Api.Setup;

/// <summary>Names of the rate-limiting policies, for <see cref="EnableRateLimitingAttribute"/>.</summary>
public static class RateLimitPolicies
{
    /// <summary>Anonymous write endpoints, limited per minute by <c>RateLimiting:WritesPerMinute</c>.</summary>
    public const string Writes = "writes";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// The write endpoints are anonymous until there is authentication, so they share one
        /// fixed-window limit. It isn't partitioned by client IP, because X-Forwarded-For is trusted
        /// from any proxy (see <see cref="ForwardedHeadersExtensions"/>) and so can be spoofed.
        /// Rejected requests get a 429, which UseStatusCodePages turns into ProblemDetails.
        /// </summary>
        public IServiceCollection AddApiRateLimiting(IConfiguration configuration)
        {
            var writesPerMinute = configuration.GetValue<int>("RateLimiting:WritesPerMinute");
            return services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter(Writes, limiter =>
                {
                    limiter.PermitLimit = writesPerMinute;
                    limiter.Window = TimeSpan.FromMinutes(1);
                });
            });
        }
    }
}
