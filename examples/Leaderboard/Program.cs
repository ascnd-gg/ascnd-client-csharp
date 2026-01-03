using Ascnd.Client;

var apiKey = Environment.GetEnvironmentVariable("ASCND_API_KEY")
    ?? throw new Exception("ASCND_API_KEY not set");
var leaderboardId = Environment.GetEnvironmentVariable("LEADERBOARD_ID")
    ?? throw new Exception("LEADERBOARD_ID not set");

using var client = new AscndClient(apiKey);

var leaderboard = await client.GetLeaderboardAsync(leaderboardId, limit: 10);

Console.WriteLine($"Top 10 Leaderboard ({leaderboard.TotalEntries} total players)\n");
Console.WriteLine("Rank  | Player             | Score");
Console.WriteLine("------+--------------------+------------");

foreach (var entry in leaderboard.Entries)
{
    var player = entry.PlayerId.Length > 18 
        ? entry.PlayerId[..18] 
        : entry.PlayerId.PadRight(18);
    Console.WriteLine($"{entry.Rank,4}  | {player} | {entry.Score,10}");
}

if (leaderboard.HasMore)
{
    Console.WriteLine("\n... and more entries");
}
