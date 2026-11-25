using System.Net;
using System.Net.Http.Json;
using System.Text;
using Alpinarc.Api.Lodges;
using Alpinarc.Api.Stages;
using Microsoft.AspNetCore.Mvc;

namespace Alpinarc.Api.Tests;

// The database is shared by every test in the run, so no test may assume the stages table is empty.
[Collection(ApiCollection.Name)]
public class StagesEndpointTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_ThenGetById_ReturnsSameStage()
    {
        using var client = factory.CreateClient();
        var from = await TestData.CreateLodge(client, "Olpererhütte");
        var to = await TestData.CreateLodge(client, "Friesenberghaus");

        var createResponse = await client.PostAsJsonAsync("/stages", new
        {
            fromLodgeId = from.Id,
            toLodgeId = to.Id,
            distanceMeters = 8200,
            ascentMeters = 650,
            descentMeters = 1320,
            durationMinutes = 210,
        }, TestRun.Token);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<StageResponse>(TestRun.Token);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(new LodgeSummary(from.Id, from.Name), created.FromLodge);
        Assert.Equal(new LodgeSummary(to.Id, to.Name), created.ToLodge);
        Assert.Equal(8200, created.DistanceMeters);
        Assert.Equal(650, created.AscentMeters);
        Assert.Equal(1320, created.DescentMeters);
        Assert.Equal(210, created.DurationMinutes);
        Assert.Equal($"/stages/{created.Id}", createResponse.Headers.Location?.AbsolutePath);

        var fetched = await client.GetFromJsonAsync<StageResponse>(createResponse.Headers.Location, TestRun.Token);

        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task List_ContainsCreatedStage_WithBothLodgeNames()
    {
        using var client = factory.CreateClient();
        var stage = await TestData.CreateStage(client);

        var stages = await client.GetFromJsonAsync<List<StageResponse>>("/stages", TestRun.Token);

        Assert.NotNull(stages);
        var listed = Assert.Single(stages, s => s.Id == stage.Id);
        Assert.Equal(stage.FromLodge.Name, listed.FromLodge.Name);
        Assert.Equal(stage.ToLodge.Name, listed.ToLodge.Name);
    }

    [Fact]
    public async Task List_IsSortedByFromLodgeName_ThenToLodgeName()
    {
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var alpha = await CreateLodgeNamed(client, $"Alpha {suffix}");
        var beta = await CreateLodgeNamed(client, $"Beta {suffix}");
        var gamma = await CreateLodgeNamed(client, $"Gamma {suffix}");
        // Created out of order, so sorting by creation would put them differently.
        var betaToAlpha = await TestData.CreateStage(client, beta, alpha);
        var alphaToGamma = await TestData.CreateStage(client, alpha, gamma);
        var alphaToBeta = await TestData.CreateStage(client, alpha, beta);

        var stages = await client.GetFromJsonAsync<List<StageResponse>>("/stages", TestRun.Token);

        Assert.NotNull(stages);
        var ids = stages.Select(s => s.Id).ToList();
        Assert.True(ids.IndexOf(alphaToBeta.Id) < ids.IndexOf(alphaToGamma.Id), "Sorted by to-lodge name");
        Assert.True(ids.IndexOf(alphaToGamma.Id) < ids.IndexOf(betaToAlpha.Id), "Sorted by from-lodge name first");
    }

    [Fact]
    public async Task Create_SameFromAndToLodge_Returns400KeyedByToLodgeId()
    {
        using var client = factory.CreateClient();
        var lodge = await TestData.CreateLodge(client);

        var response = await PostStage(client, lodge.Id, lodge.Id);

        var problem = await AssertBadRequest(response);
        Assert.Contains("toLodgeId", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_UnknownFromLodge_Returns400KeyedByFromLodgeId()
    {
        using var client = factory.CreateClient();
        var lodge = await TestData.CreateLodge(client);

        var response = await PostStage(client, Guid.NewGuid(), lodge.Id);

        var problem = await AssertBadRequest(response);
        Assert.Equal(["fromLodgeId"], problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_UnknownToLodge_Returns400KeyedByToLodgeId()
    {
        using var client = factory.CreateClient();
        var lodge = await TestData.CreateLodge(client);

        var response = await PostStage(client, lodge.Id, Guid.NewGuid());

        var problem = await AssertBadRequest(response);
        Assert.Equal(["toLodgeId"], problem.Errors.Keys);
    }

    [Theory]
    [InlineData(0, 650, 1320, 210, "distanceMeters")]
    [InlineData(100_001, 650, 1320, 210, "distanceMeters")]
    [InlineData(8200, -1, 1320, 210, "ascentMeters")]
    [InlineData(8200, 10_001, 1320, 210, "ascentMeters")]
    [InlineData(8200, 650, -1, 210, "descentMeters")]
    [InlineData(8200, 650, 10_001, 210, "descentMeters")]
    [InlineData(8200, 650, 1320, 0, "durationMinutes")]
    [InlineData(8200, 650, 1320, 1441, "durationMinutes")]
    public async Task Create_OutOfRangeNumber_Returns400KeyedByField(
        int distanceMeters, int ascentMeters, int descentMeters, int durationMinutes, string field)
    {
        using var client = factory.CreateClient();
        var from = await TestData.CreateLodge(client);
        var to = await TestData.CreateLodge(client);

        var response = await PostStage(
            client, from.Id, to.Id, distanceMeters, ascentMeters, descentMeters, durationMinutes);

        var problem = await AssertBadRequest(response);
        Assert.Contains(field, problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_NumbersAtTheLimits_AreAllowed()
    {
        using var client = factory.CreateClient();
        var from = await TestData.CreateLodge(client);
        var to = await TestData.CreateLodge(client);

        var lowest = await PostStage(client, from.Id, to.Id, 1, 0, 0, 1);
        var highest = await PostStage(client, from.Id, to.Id, 100_000, 10_000, 10_000, 1440);

        Assert.Equal(HttpStatusCode.Created, lowest.StatusCode);
        Assert.Equal(HttpStatusCode.Created, highest.StatusCode);
    }

    [Fact]
    public async Task Create_MissingField_Returns400()
    {
        using var client = factory.CreateClient();
        var from = await TestData.CreateLodge(client);
        var to = await TestData.CreateLodge(client);
        var json = $$"""
            { "fromLodgeId": "{{from.Id}}", "toLodgeId": "{{to.Id}}", "distanceMeters": 8200, "ascentMeters": 650, "descentMeters": 1320 }
            """;

        var response = await client.PostAsync(
            "/stages", new StringContent(json, Encoding.UTF8, "application/json"), TestRun.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NumberAsString_Returns400()
    {
        using var client = factory.CreateClient();
        var from = await TestData.CreateLodge(client);
        var to = await TestData.CreateLodge(client);
        var json = $$"""
            { "fromLodgeId": "{{from.Id}}", "toLodgeId": "{{to.Id}}", "distanceMeters": "8200", "ascentMeters": 650, "descentMeters": 1320, "durationMinutes": 210 }
            """;

        var response = await client.PostAsync(
            "/stages", new StringContent(json, Encoding.UTF8, "application/json"), TestRun.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/stages/{Guid.NewGuid()}", TestRun.Token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_TwoStagesBetweenSameLodges_AreBothAllowed()
    {
        using var client = factory.CreateClient();
        var from = await TestData.CreateLodge(client);
        var to = await TestData.CreateLodge(client);

        var normal = await TestData.CreateStage(client, from, to, durationMinutes: 210);
        var glacier = await TestData.CreateStage(client, from, to, durationMinutes: 300);

        Assert.NotEqual(normal.Id, glacier.Id);
    }

    [Fact]
    public async Task Create_BothDirectionsBetweenSameLodges_AreBothAllowed()
    {
        using var client = factory.CreateClient();
        var a = await TestData.CreateLodge(client);
        var b = await TestData.CreateLodge(client);

        var there = await TestData.CreateStage(client, a, b, ascentMeters: 650, descentMeters: 1320);
        var back = await TestData.CreateStage(client, b, a, ascentMeters: 1320, descentMeters: 650);

        Assert.Equal(a.Id, there.FromLodge.Id);
        Assert.Equal(b.Id, back.FromLodge.Id);
    }

    private static Task<HttpResponseMessage> PostStage(
        HttpClient client, Guid fromLodgeId, Guid toLodgeId, int distanceMeters = 8200, int ascentMeters = 650,
        int descentMeters = 1320, int durationMinutes = 210) =>
        client.PostAsJsonAsync("/stages", new
        {
            fromLodgeId,
            toLodgeId,
            distanceMeters,
            ascentMeters,
            descentMeters,
            durationMinutes,
        }, TestRun.Token);

    private static async Task<ValidationProblemDetails> AssertBadRequest(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestRun.Token);
        Assert.NotNull(problem);
        return problem;
    }

    private static async Task<LodgeResponse> CreateLodgeNamed(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/lodges", new { name }, TestRun.Token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LodgeResponse>(TestRun.Token))!;
    }
}
