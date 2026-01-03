using Microsoft.Extensions.Logging;
using Xunit;

namespace Ascnd.Client.Tests;

public class AscndClientLoggingTests
{
    private const string ValidApiKey = "test-api-key";

    [Fact]
    public void Constructor_WithoutLogger_CreatesClientSuccessfully()
    {
        var options = new AscndClientOptions(ValidApiKey)
        {
            Logger = null
        };

        using var client = new AscndClient(options);

        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithLogger_CreatesClientSuccessfully()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        var logger = loggerFactory.CreateLogger<AscndClient>();

        var options = new AscndClientOptions(ValidApiKey)
        {
            Logger = logger
        };

        using var client = new AscndClient(options);

        Assert.NotNull(client);
    }

    [Fact]
    public void Dispose_WithLogger_DoesNotThrow()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        var logger = loggerFactory.CreateLogger<AscndClient>();

        var options = new AscndClientOptions(ValidApiKey)
        {
            Logger = logger
        };

        var client = new AscndClient(options);

        var exception = Record.Exception(() => client.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public async Task DisposeAsync_WithLogger_DoesNotThrow()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        var logger = loggerFactory.CreateLogger<AscndClient>();

        var options = new AscndClientOptions(ValidApiKey)
        {
            Logger = logger
        };

        var client = new AscndClient(options);

        var exception = await Record.ExceptionAsync(async () => await client.DisposeAsync());

        Assert.Null(exception);
    }

    [Fact]
    public void Logger_PropertyOnOptions_CanBeSetAndRetrieved()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        var logger = loggerFactory.CreateLogger<AscndClient>();

        var options = new AscndClientOptions(ValidApiKey)
        {
            Logger = logger
        };

        Assert.Same(logger, options.Logger);
    }

    [Fact]
    public void Logger_PropertyOnOptions_DefaultsToNull()
    {
        var options = new AscndClientOptions(ValidApiKey);

        Assert.Null(options.Logger);
    }

    [Fact]
    public async Task SubmitScoreAsync_WithLogger_AfterDispose_ThrowsObjectDisposedException()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        var logger = loggerFactory.CreateLogger<AscndClient>();

        var options = new AscndClientOptions(ValidApiKey)
        {
            Logger = logger
        };

        var client = new AscndClient(options);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.SubmitScoreAsync("leaderboard", "player", 100));
    }
}
