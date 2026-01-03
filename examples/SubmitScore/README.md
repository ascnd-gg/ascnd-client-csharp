# Submit Score Example

Demonstrates submitting a score to an Ascnd leaderboard.

## Build

```bash
dotnet build
```

## Environment Variables

| Variable | Description |
|----------|-------------|
| `ASCND_API_KEY` | Your Ascnd API key |
| `LEADERBOARD_ID` | Target leaderboard ID |

## Run

```bash
export ASCND_API_KEY=your_api_key
export LEADERBOARD_ID=your_leaderboard_id
dotnet run
```

Or on Windows PowerShell:

```powershell
$env:ASCND_API_KEY = "your_api_key"
$env:LEADERBOARD_ID = "your_leaderboard_id"
dotnet run
```

## Expected Output

```
Score submitted!
  Rank: #42
  New personal best: Yes!
```
