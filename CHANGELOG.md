# Ascnd.Client Changelog

## 1.1.1

### Fixed

- Re-release to correct publishing issue with v1.1.0

## 1.1.0

### Added

- Microsoft.Extensions.Logging support - optional `ILogger<AscndClient>` can be passed via `AscndClientOptions.Logger`
- `IAsyncDisposable` implementation for .NET 6.0+ with proper async channel shutdown
- .NET 10.0 target framework support

### Changed

- Improved thread-safe disposal using `Interlocked.Exchange` pattern
- CI/CD workflows now run tests before pack and publish

### Fixed

- Disposal is now fully thread-safe for concurrent access scenarios

## 1.0.0

### Initial Release

- Initial release of `Ascnd.Client`
- Migrated to new GitHub organization (ascnd-gg)
- Full support for .NET Standard 2.0, .NET 6.0, and .NET 8.0
- Unity 2019.4+ compatible

### Features

- `SubmitScoreAsync()` - Submit player scores to leaderboards
- `GetLeaderboardAsync()` - Retrieve top scores with pagination
- `GetPlayerRankAsync()` - Get a specific player's rank and percentile
- Convenience overloads for common use cases
- Support for metadata and idempotency keys
- Comprehensive error handling with `AscndApiException`
