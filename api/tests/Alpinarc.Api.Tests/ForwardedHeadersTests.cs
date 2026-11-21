using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace Alpinarc.Api.Tests;

// Railway terminates HTTPS at its edge and forwards plain HTTP with X-Forwarded-Proto.
[Collection(ApiCollection.Name)]
public class ForwardedHeadersTests(ApiFactory factory)
{
    [Fact]
    public async Task Request_WithoutForwardedProto_IsRedirectedToHttps()
    {
        using var client = CreateProductionClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
    }

    [Fact]
    public async Task Request_ForwardedAsHttps_IsNotRedirected()
    {
        using var client = CreateProductionClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Forwarded-Proto", "https");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Production turns on HTTPS redirection. An https_port makes it actually redirect,
    // which is what would cause a redirect loop behind a proxy without forwarded headers.
    private HttpClient CreateProductionClient() => factory
        .WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseSetting("https_port", "443"))
        .CreateClient(new() { AllowAutoRedirect = false });
}
