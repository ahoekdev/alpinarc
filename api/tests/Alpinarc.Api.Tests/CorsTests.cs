using Microsoft.AspNetCore.Hosting;

namespace Alpinarc.Api.Tests;

[Collection(ApiCollection.Name)]
public class CorsTests(ApiFactory factory)
{
    private const string WebOrigin = "https://web.example.com";

    // Railway sets the origin as Cors__AllowedOrigins__0, which configuration reads as
    // the indexed key Cors:AllowedOrigins:0.
    [Theory]
    [InlineData(WebOrigin, WebOrigin)]
    [InlineData("https://other.example.com", null)]
    public async Task Preflight_AllowsOnlyConfiguredOrigin(string origin, string? expectedAllowOrigin)
    {
        using var client = factory
            .WithWebHostBuilder(builder => builder
                .UseEnvironment("Production")
                .UseSetting("Cors:AllowedOrigins:0", WebOrigin))
            .CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/lodges");
        request.Headers.Add("X-Forwarded-Proto", "https");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        var response = await client.SendAsync(request);

        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowOrigin);
        Assert.Equal(expectedAllowOrigin, allowOrigin?.SingleOrDefault());
    }
}
