using Ascnd.Client;

var apiKey = Environment.GetEnvironmentVariable("ASCND_API_KEY")
    ?? throw new Exception("ASCND_API_KEY not set");
var leaderboardId = Environment.GetEnvironmentVariable("LEADERBOARD_ID")
    ?? throw new Exception("LEADERBOARD_ID not set");

using var client = new AscndClient(apiKey);

var result = await client.SubmitScoreAsync(leaderboardId, "player_example_001", 42500);

Console.WriteLine("Score submitted!");
Console.WriteLine($"  Rank: #{result.Rank}");
Console.WriteLine($"  New personal best: {(result.IsNewBest ? "Yes!" : "No")}");
