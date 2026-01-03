using System.Text;
using System.Text.Json;
using Ascnd.Client;
using Ascnd.Client.Grpc;
using Google.Protobuf;

var apiKey = Environment.GetEnvironmentVariable("ASCND_API_KEY")
    ?? throw new Exception("ASCND_API_KEY not set");
var leaderboardId = Environment.GetEnvironmentVariable("LEADERBOARD_ID")
    ?? throw new Exception("LEADERBOARD_ID not set");

using var client = new AscndClient(apiKey);

// Submit score with metadata
var metadata = new
{
    character = "warrior",
    level = 15,
    powerups = new[] { "speed", "shield" }
};

var request = new SubmitScoreRequest
{
    LeaderboardId = leaderboardId,
    PlayerId = "player_meta_001",
    Score = 75000,
    Metadata = ByteString.CopyFrom(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata)))
};

var result = await client.SubmitScoreAsync(request);

Console.WriteLine($"Score submitted with metadata! Rank: #{result.Rank}\n");

// Fetch current period leaderboard
var leaderboardRequest = new GetLeaderboardRequest
{
    LeaderboardId = leaderboardId,
    Limit = 5,
    Period = "current"
};

var leaderboard = await client.GetLeaderboardAsync(leaderboardRequest);

Console.WriteLine($"Current Period: {leaderboard.PeriodStart}");
if (leaderboard.HasPeriodEnd)
{
    Console.WriteLine($"Ends: {leaderboard.PeriodEnd}");
}
Console.WriteLine("\nTop 5 with metadata:\n");

foreach (var entry in leaderboard.Entries)
{
    Console.WriteLine($"#{entry.Rank} {entry.PlayerId}: {entry.Score}");
    if (entry.HasMetadata)
    {
        var json = Encoding.UTF8.GetString(entry.Metadata.ToByteArray());
        Console.WriteLine($"   Metadata: {json}");
    }
}
