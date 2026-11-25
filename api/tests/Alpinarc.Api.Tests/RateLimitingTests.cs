using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Alpinarc.Api.Tests;

[Collection(ApiCollection.Name)]
public class RateLimitingTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_OverWriteLimit_Returns429ProblemDetails()
    {
        using var limited = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("RateLimiting:WritesPerMinute", "2"));
        using var client = limited.CreateClient();

        for (var i = 0; i < 2; i++)
        {
            var allowed = await client.PostAsJsonAsync("/lodges", new { name = TestNames.Unique("Rate limit hut") });
            Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
        }

        var rejected = await client.PostAsJsonAsync("/lodges", new { name = TestNames.Unique("Rate limit hut") });

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal("application/problem+json", rejected.Content.Headers.ContentType?.MediaType);
        var problem = await rejected.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal((int)HttpStatusCode.TooManyRequests, problem?.Status);
    }

    [Fact]
    public async Task Reads_AreNotLimited()
    {
        using var limited = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("RateLimiting:WritesPerMinute", "1"));
        using var client = limited.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/lodges");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
