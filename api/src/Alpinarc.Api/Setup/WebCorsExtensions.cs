using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Alpinarc.Api.Setup;

/// <summary>The CORS policy that lets the web app call the API.</summary>
public static class WebCorsExtensions
{
    private const string PolicyName = "Web";

    extension(IServiceCollection services)
    {
        /// <summary>Adds the CORS policy for the origins in <see cref="WebCorsOptions"/>, validated at startup.</summary>
        public IServiceCollection AddWebCors()
        {
            services.AddOptions<WebCorsOptions>()
                .BindConfiguration(WebCorsOptions.SectionName)
                .ValidateOnStartUnlessGeneratingOpenApi();
            services.AddOptions<CorsOptions>()
                .Configure<IOptions<WebCorsOptions>>((options, web) => options.AddPolicy(PolicyName, policy => policy
                    .WithOrigins(web.Value.AllowedOrigins)
                    // Only what the web app's fetch client sends: reads, creates, and the JSON
                    // Content-Type. Widen this when the API gets other methods or headers.
                    .WithMethods(HttpMethods.Get, HttpMethods.Post)
                    .WithHeaders(HeaderNames.ContentType)));

            return services.AddCors();
        }
    }

    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseWebCors() => app.UseCors(PolicyName);
    }
}
