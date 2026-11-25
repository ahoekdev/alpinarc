using Alpinarc.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Alpinarc.Api.Tests;

/// <summary>
/// Runs the API in memory against a throwaway Postgres container with the migrations applied.
/// Shared by every test class in the <see cref="ApiCollection"/>, so the container starts once per test run.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Same major version as docker-compose.yml; uuidv7() needs Postgres 18+.
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AlpinarcDbContext>();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder
        .UseSetting("ConnectionStrings:Alpinarc", _postgres.GetConnectionString())
        // The app doesn't start without an origin; Development has one, Production doesn't.
        .UseSetting("Cors:AllowedOrigins:0", "https://web.example.com")
        // Every test shares this one limit, so keep it out of the way. RateLimitingTests
        // sets a low limit to test it.
        .UseSetting("RateLimiting:WritesPerMinute", "10000");

    // xunit v3's IAsyncLifetime disposes through IAsyncDisposable, which the factory implements.
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
