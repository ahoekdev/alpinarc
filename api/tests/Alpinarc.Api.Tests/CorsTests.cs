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
        var response = await SendPreflight(origin, "POST", "content-type");

        Assert.Equal(expectedAllowOrigin, GetHeader(response, "Access-Control-Allow-Origin"));
    }

    // The policy allows only what the web app sends: GET and POST, with a JSON Content-Type.
    // The API always answers with these lists, and the browser blocks a request that asks
    // for anything else (here DELETE and X-Custom).
    [Fact]
    public async Task Preflight_AllowsOnlyWebAppMethodsAndHeaders()
    {
        var response = await SendPreflight(WebOrigin, "DELETE", "x-custom");

        Assert.Equal("GET,POST", GetHeader(response, "Access-Control-Allow-Methods"));
        Assert.Equal("Content-Type", GetHeader(response, "Access-Control-Allow-Headers"));
    }

    private async Task<HttpResponseMessage> SendPreflight(string origin, string method, string? headers)
    {
        using var web = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseSetting("Cors:AllowedOrigins:0", WebOrigin));
        using var client = web.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/lodges");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", method);
        if (headers is not null)
        {
            request.Headers.Add("Access-Control-Request-Headers", headers);
        }

        return await client.SendAsync(request);
    }

    private static string? GetHeader(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.SingleOrDefault() : null;
}
