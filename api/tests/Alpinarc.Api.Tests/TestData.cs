using System.Net.Http.Json;
using Alpinarc.Api.Lodges;
using Alpinarc.Api.Stages;
using Alpinarc.Api.Tours;

namespace Alpinarc.Api.Tests;

/// <summary>Creates the data a test needs through the API, failing the test if that doesn't work.</summary>
internal static class TestData
{
    public static async Task<LodgeResponse> CreateLodge(HttpClient client, string name = "Hut")
    {
        var response = await client.PostAsJsonAsync("/lodges", new { name = TestNames.Unique(name) }, TestRun.Token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LodgeResponse>(TestRun.Token))!;
    }

    public static async Task<StageResponse> CreateStage(
        HttpClient client,
        LodgeResponse from,
        LodgeResponse to,
        int distanceMeters = 8200,
        int ascentMeters = 650,
        int descentMeters = 1320,
        int durationMinutes = 210)
    {
        var response = await client.PostAsJsonAsync("/stages", new
        {
            fromLodgeId = from.Id,
            toLodgeId = to.Id,
            distanceMeters,
            ascentMeters,
            descentMeters,
            durationMinutes,
        }, TestRun.Token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StageResponse>(TestRun.Token))!;
    }

    /// <summary>A stage between two new lodges.</summary>
    public static async Task<StageResponse> CreateStage(
        HttpClient client, int distanceMeters = 8200, int ascentMeters = 650, int descentMeters = 1320,
        int durationMinutes = 210)
    {
        var from = await CreateLodge(client);
        var to = await CreateLodge(client);
        return await CreateStage(client, from, to, distanceMeters, ascentMeters, descentMeters, durationMinutes);
    }

    public static async Task<TourResponse> CreateTour(HttpClient client, params StageResponse[] stages)
    {
        var response = await client.PostAsJsonAsync("/tours", new
        {
            name = TestNames.Unique("Tour"),
            stageIds = stages.Select(s => s.Id),
        }, TestRun.Token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TourResponse>(TestRun.Token))!;
    }
}
