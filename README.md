# Ascnd.Client

> [!WARNING]
> This project is under active development. Expect bugs. Please report issues via the [issue tracker](../../issues).

Official C# client library for the [Ascnd](https://ascnd.gg) leaderboard API.

## Features

- gRPC-based API with generated protobuf types
- Cross-platform support: .NET Standard 2.0, .NET 6.0, .NET 8.0, .NET 10.0
- Unity compatible (2019.4+) using Grpc.Core for .NET Standard 2.0
- Async/await support
- Built-in anticheat result handling
- Bracket and view support for segmented leaderboards
- Global rank tracking across views

## Installation

### NuGet (.NET)

```bash
dotnet add package Ascnd.Client
```

Or via Package Manager:

```powershell
Install-Package Ascnd.Client
```

### Unity Package Manager

1. Download the latest release from [GitHub Releases](https://github.com/ascnd-gg/ascnd-client-csharp/releases)
2. In Unity, go to **Window > Package Manager**
3. Click the **+** button and select **Add package from tarball...**
4. Select the downloaded `.tgz` file

Or add directly to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ascnd.client": "https://github.com/ascnd-gg/ascnd-client-csharp.git#v1.0.0"
  }
}
```

## Quick Start

```csharp
using Ascnd.Client;
using Ascnd.Client.Grpc;
using Google.Protobuf;

// Create client with your API key
using var client = new AscndClient("your-api-key");

// Submit a score
var result = await client.SubmitScoreAsync("weekly-highscores", "player123", 42500);
Console.WriteLine($"New rank: #{result.Rank}");

// Get leaderboard
var leaderboard = await client.GetLeaderboardAsync("weekly-highscores", limit: 10);
foreach (var entry in leaderboard.Entries)
{
    Console.WriteLine($"#{entry.Rank}: {entry.PlayerId} - {entry.Score}");
}

// Get player rank with bracket and global rank info
var request = new GetPlayerRankRequest
{
    LeaderboardId = "weekly-highscores",
    PlayerId = "player123"
};
var playerRank = await client.GetPlayerRankAsync(request);
if (playerRank.HasRank)
{
    Console.WriteLine($"You are #{playerRank.Rank} ({playerRank.Percentile})");
    if (playerRank.HasGlobalRank)
    {
        Console.WriteLine($"Global rank: #{playerRank.GlobalRank}");
    }
}
```

## Examples

Stand-alone example projects are available in the [`examples/`](./examples) directory:

| Example | Description |
|---------|-------------|
| [`SubmitScore`](./examples/SubmitScore) | Submit a score and display the resulting rank |
| [`Leaderboard`](./examples/Leaderboard) | Fetch and display the top 10 leaderboard entries |
| [`MetadataPeriods`](./examples/MetadataPeriods) | Submit scores with metadata and query by time period |

Each example is a self-contained .NET project. To run:

```bash
cd examples/SubmitScore
export ASCND_API_KEY=your_api_key
export LEADERBOARD_ID=your_leaderboard_id
dotnet run
```

## Configuration

### Basic Configuration

```csharp
var client = new AscndClient("your-api-key");
```

### Advanced Configuration

```csharp
var options = new AscndClientOptions
{
    ApiKey = "your-api-key",
    BaseUrl = "https://api.ascnd.gg",  // Default
    TimeoutSeconds = 30                 // Default
};

var client = new AscndClient(options);
```

## API Reference

### SubmitScoreAsync

Submit a player's score to a leaderboard using generated protobuf types.

```csharp
using Ascnd.Client.Grpc;
using Google.Protobuf;
using System.Text;
using System.Text.Json;

// Simple overload
var result = await client.SubmitScoreAsync("leaderboard-id", "player-id", 42500);

// Full request object with metadata as ByteString
var metadata = new { character = "warrior", level = 15 };
var request = new SubmitScoreRequest
{
    LeaderboardId = "weekly-highscores",
    PlayerId = "player123",
    Score = 42500,
    Metadata = ByteString.CopyFrom(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata))),
    IdempotencyKey = "game-session-123-final"  // Prevents duplicate submissions
};
var result = await client.SubmitScoreAsync(request);

Console.WriteLine($"Score ID: {result.ScoreId}");
Console.WriteLine($"Rank: #{result.Rank}");
Console.WriteLine($"New best: {result.IsNewBest}");
Console.WriteLine($"Deduplicated: {result.WasDeduplicated}");
```

### GetLeaderboardAsync

Retrieve leaderboard entries with bracket information.

```csharp
using Ascnd.Client.Grpc;

// Simple overload
var leaderboard = await client.GetLeaderboardAsync("leaderboard-id", limit: 25);

// Full request object with view filtering
var request = new GetLeaderboardRequest
{
    LeaderboardId = "weekly-highscores",
    Limit = 25,
    Period = "current",      // "current", "previous", or ISO timestamp
    ViewSlug = "na-region"   // Optional: filter by metadata view
};
var leaderboard = await client.GetLeaderboardAsync(request);

Console.WriteLine($"Period: {leaderboard.PeriodStart}");
if (leaderboard.HasView)
{
    Console.WriteLine($"View: {leaderboard.View.Name} ({leaderboard.View.Slug})");
}

foreach (var entry in leaderboard.Entries)
{
    Console.WriteLine($"#{entry.Rank}: {entry.PlayerId} - {entry.Score}");

    // Access bracket information
    if (entry.HasBracket)
    {
        Console.WriteLine($"   Bracket: {entry.Bracket.Name} ({entry.Bracket.Color})");
    }

    // Access metadata
    if (entry.HasMetadata)
    {
        var json = Encoding.UTF8.GetString(entry.Metadata.ToByteArray());
        Console.WriteLine($"   Metadata: {json}");
    }
}
```


### Pagination

The leaderboard API uses cursor-based pagination for efficient traversal of large result sets.

```csharp
// First page
var leaderboard = await client.GetLeaderboardAsync("leaderboard-id", limit: 25);

// Next page using cursor
if (leaderboard.HasMore)
{
    var nextPage = await client.GetLeaderboardAsync(new GetLeaderboardRequest
    {
        LeaderboardId = "leaderboard-id",
        Limit = 25,
        Cursor = leaderboard.NextCursor
    });
}
```

#### Random Access with AroundRank

Use `AroundRank` to fetch entries centered around a specific rank position:

```csharp
// Get entries around rank 500 (useful for "see players near you" features)
var request = new GetLeaderboardRequest
{
    LeaderboardId = "weekly-highscores",
    Limit = 21,        // Gets 10 above, the target rank, and 10 below
    AroundRank = 500
};
var leaderboard = await client.GetLeaderboardAsync(request);

// Note: AroundRank returns a window of entries centered on the specified rank
// The cursor can be used to paginate from this position
```

### GetPlayerRankAsync

Get a specific player's rank with global rank and bracket info.

```csharp
using Ascnd.Client.Grpc;

// Simple overload
var rank = await client.GetPlayerRankAsync("leaderboard-id", "player-id");

// Full request object with view filtering
var request = new GetPlayerRankRequest
{
    LeaderboardId = "weekly-highscores",
    PlayerId = "player123",
    Period = "current",
    ViewSlug = "na-region"  // Optional: get rank within a specific view
};
var rank = await client.GetPlayerRankAsync(request);

if (rank.HasRank)
{
    Console.WriteLine($"Rank: #{rank.Rank}");
    Console.WriteLine($"Score: {rank.Score}");
    Console.WriteLine($"Best Score: {rank.BestScore}");
    Console.WriteLine($"Percentile: {rank.Percentile}");

    // Global rank (across all views)
    if (rank.HasGlobalRank)
    {
        Console.WriteLine($"Global Rank: #{rank.GlobalRank}");
    }

    // Bracket information
    if (rank.HasBracket)
    {
        Console.WriteLine($"Bracket: {rank.Bracket.Name}");
    }

    // View information
    if (rank.HasView)
    {
        Console.WriteLine($"View: {rank.View.Name}");
    }
}
```

## Anticheat

The SDK provides built-in anticheat result handling. When submitting scores, you can check if the score passed anticheat validation and handle violations accordingly.

```csharp
var result = await client.SubmitScoreAsync("leaderboard-id", "player123", 42500);

// Check anticheat results
if (result.HasAnticheat)
{
    var anticheat = result.Anticheat;

    if (anticheat.Passed)
    {
        Console.WriteLine("Score passed anticheat validation");
    }
    else
    {
        Console.WriteLine($"Anticheat action: {anticheat.Action}");

        // Iterate through violations
        foreach (var violation in anticheat.Violations)
        {
            Console.WriteLine($"  - {violation.FlagType}: {violation.Reason}");
        }
    }
}
```

### Violation Types

- `bounds_exceeded` - Score value outside configured bounds
- `velocity_exceeded` - Score submitted too quickly after previous score
- `duplicate_idempotency` - Duplicate idempotency key detected
- `missing_idempotency_key` - Required idempotency key not provided

## Brackets

Brackets allow you to categorize players into tiers based on their ranking (e.g., Bronze, Silver, Gold, Diamond). Bracket information is included in leaderboard entries and player rank responses.

```csharp
// Get leaderboard with bracket info
var leaderboard = await client.GetLeaderboardAsync("ranked-ladder", limit: 10);

foreach (var entry in leaderboard.Entries)
{
    if (entry.HasBracket)
    {
        // Display with bracket color (hex format, e.g., "#FFD700" for Gold)
        Console.WriteLine($"#{entry.Rank} [{entry.Bracket.Name}] {entry.PlayerId}: {entry.Score}");
        Console.WriteLine($"   Color: {entry.Bracket.Color}");
    }
}

// Get player's bracket
var playerRank = await client.GetPlayerRankAsync("ranked-ladder", "player123");
if (playerRank.HasBracket)
{
    Console.WriteLine($"Your bracket: {playerRank.Bracket.Name}");
    Console.WriteLine($"Bracket ID: {playerRank.Bracket.Id}");
}
```

## Metadata Views

Views allow you to filter leaderboards by metadata fields (e.g., region, game mode, character class). Use the `ViewSlug` parameter to query specific segments.

```csharp
// Get leaderboard for a specific view
var request = new GetLeaderboardRequest
{
    LeaderboardId = "global-highscores",
    Limit = 10,
    ViewSlug = "warrior-class"  // Filter by character class
};
var leaderboard = await client.GetLeaderboardAsync(request);

// View info is included in the response
if (leaderboard.HasView)
{
    Console.WriteLine($"Showing: {leaderboard.View.Name}");
}

// Get player rank within a view, plus their global rank
var rankRequest = new GetPlayerRankRequest
{
    LeaderboardId = "global-highscores",
    PlayerId = "player123",
    ViewSlug = "warrior-class"
};
var rank = await client.GetPlayerRankAsync(rankRequest);

if (rank.HasRank)
{
    Console.WriteLine($"View Rank: #{rank.Rank}");        // Rank within warriors
    if (rank.HasGlobalRank)
    {
        Console.WriteLine($"Global Rank: #{rank.GlobalRank}");  // Rank across all classes
    }
}
```

## Unity Usage

The SDK uses `Grpc.Core` for .NET Standard 2.0 compatibility, which is required for Unity 2019.4+. This provides native gRPC support without requiring .NET 6+ features.

### Basic Unity Example

```csharp
using UnityEngine;
using Ascnd.Client;
using Ascnd.Client.Grpc;
using Google.Protobuf;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class LeaderboardManager : MonoBehaviour
{
    private AscndClient _client;

    void Awake()
    {
        _client = new AscndClient("your-api-key");
    }

    void OnDestroy()
    {
        _client?.Dispose();
    }

    public async Task SubmitScore(string playerId, long score, string character)
    {
        try
        {
            // Submit with metadata
            var metadata = new { character = character, timestamp = System.DateTime.UtcNow };
            var request = new SubmitScoreRequest
            {
                LeaderboardId = "game-highscores",
                PlayerId = playerId,
                Score = score,
                Metadata = ByteString.CopyFrom(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata)))
            };

            var result = await _client.SubmitScoreAsync(request);
            Debug.Log($"Score submitted! New rank: #{result.Rank}");

            if (result.IsNewBest)
            {
                Debug.Log("New personal best!");
            }

            // Check anticheat
            if (result.HasAnticheat && !result.Anticheat.Passed)
            {
                Debug.LogWarning($"Anticheat flagged: {result.Anticheat.Action}");
            }
        }
        catch (AscndApiException ex)
        {
            Debug.LogError($"API error: {ex.Message} (Status: {ex.StatusCode})");
        }
    }

    public async Task<LeaderboardEntry[]> GetTopPlayers(int count = 10)
    {
        var leaderboard = await _client.GetLeaderboardAsync("game-highscores", limit: count);
        return leaderboard.Entries.ToArray();
    }
}
```

### Unity gRPC Notes

- The SDK automatically uses `Grpc.Core` when targeting .NET Standard 2.0
- Channel connections are managed internally and cleaned up on `Dispose()`
- For WebGL builds, additional configuration may be required (gRPC-Web proxy)

## Error Handling

```csharp
using Grpc.Core;

try
{
    var result = await client.SubmitScoreAsync("leaderboard-id", "player-id", 42500);
}
catch (AscndApiException ex)
{
    // API returned an error
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.ResponseBody}");

    // gRPC status code is also available
    if (ex.GrpcStatusCode.HasValue)
    {
        Console.WriteLine($"gRPC Status: {ex.GrpcStatusCode}");
    }

    // Handle specific errors
    switch (ex.StatusCode)
    {
        case 401:
            Console.WriteLine("Invalid API key");
            break;
        case 429:
            Console.WriteLine("Rate limit exceeded");
            break;
        case 404:
            Console.WriteLine("Leaderboard not found");
            break;
    }
}
catch (RpcException ex)
{
    // Low-level gRPC error
    Console.WriteLine($"gRPC error: {ex.Status.Detail}");
}
catch (TaskCanceledException)
{
    // Request timed out
    Console.WriteLine("Request timed out");
}
```

## Links

- [Documentation](https://docs.ascnd.gg/sdks/csharp)
- [GitHub](https://github.com/ascnd-gg/ascnd-client-csharp)
- [NuGet](https://www.nuget.org/packages/Ascnd.Client)

## License

MIT License - see [LICENSE](LICENSE) for details.
