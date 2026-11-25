using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Alpinarc.Api.Tests;

// Misconfiguration stops the app at startup, so a broken deploy never goes live.
[Collection(ApiCollection.Name)]
public class StartupValidationTests(ApiFactory factory)
{
    [Fact]
    public void MissingConnectionString_FailsAtStartup()
    {
        using var misconfigured = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:Alpinarc", ""));

        var exception = Assert.Throws<OptionsValidationException>(() => misconfigured.CreateClient());
        Assert.Contains("Alpinarc", exception.Message);
    }

    // Railway's reference https://${{web.RAILWAY_PUBLIC_DOMAIN}} gives "https://" when the web
    // service has no domain.
    [Theory]
    [InlineData("")]
    [InlineData("https://")]
    [InlineData("web.example.com")]
    public void InvalidCorsOrigin_FailsAtStartup(string origin)
    {
        using var misconfigured = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseSetting("Cors:AllowedOrigins:0", origin));

        var exception = Assert.Throws<OptionsValidationException>(() => misconfigured.CreateClient());
        Assert.Contains("AllowedOrigins", exception.Message);
    }
}
