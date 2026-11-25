using Microsoft.AspNetCore.HttpOverrides;

namespace Alpinarc.Api;

public static class ForwardedHeadersExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Railway terminates HTTPS at its edge and forwards plain HTTP. Reading X-Forwarded-Proto
        /// makes the request count as HTTPS again, so UseHttpsRedirection doesn't redirect it in a
        /// loop. Railway's proxy addresses aren't fixed, so the known networks and proxies are
        /// cleared to trust it. The API is only reachable through that proxy.
        /// </summary>
        public IServiceCollection AddRailwayForwardedHeaders() =>
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
    }
}
