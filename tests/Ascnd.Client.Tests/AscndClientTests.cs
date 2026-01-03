using Xunit;

namespace Ascnd.Client.Tests;

public class AscndClientTests
{
    private const string ValidApiKey = "test-api-key";

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AscndClient((string)null!));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AscndClient((AscndClientOptions)null!));
    }

    [Fact]
    public void Constructor_WithInvalidOptions_ThrowsInvalidOperationException()
    {
        var options = new AscndClientOptions { ApiKey = "" };

        Assert.Throws<InvalidOperationException>(() => new AscndClient(options));
    }

    [Fact]
    public void Constructor_WithValidApiKey_CreatesClient()
    {
        using var client = new AscndClient(ValidApiKey);

        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithValidOptions_CreatesClient()
    {
        var options = new AscndClientOptions(ValidApiKey);

        using var client = new AscndClient(options);

        Assert.NotNull(client);
    }

    [Fact]
    public void Dispose_WhenCalled_DoesNotThrow()
    {
        var client = new AscndClient(ValidApiKey);

        var exception = Record.Exception(() => client.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        var client = new AscndClient(ValidApiKey);

        client.Dispose();
        var exception = Record.Exception(() => client.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public async Task SubmitScoreAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var client = new AscndClient(ValidApiKey);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.SubmitScoreAsync("leaderboard", "player", 100));
    }

    [Fact]
    public async Task GetLeaderboardAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var client = new AscndClient(ValidApiKey);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.GetLeaderboardAsync("leaderboard"));
    }

    [Fact]
    public async Task GetPlayerRankAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var client = new AscndClient(ValidApiKey);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.GetPlayerRankAsync("leaderboard", "player"));
    }

    [Fact]
    public async Task SubmitScoreAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        using var client = new AscndClient(ValidApiKey);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.SubmitScoreAsync((Grpc.SubmitScoreRequest)null!));
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        using var client = new AscndClient(ValidApiKey);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.GetLeaderboardAsync((Grpc.GetLeaderboardRequest)null!));
    }

    [Fact]
    public async Task GetPlayerRankAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        using var client = new AscndClient(ValidApiKey);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.GetPlayerRankAsync((Grpc.GetPlayerRankRequest)null!));
    }

    [Fact]
    public async Task DisposeAsync_WhenCalled_DoesNotThrow()
    {
        var client = new AscndClient(ValidApiKey);

        var exception = await Record.ExceptionAsync(async () => await client.DisposeAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task DisposeAsync_WhenCalledMultipleTimes_DoesNotThrow()
    {
        var client = new AscndClient(ValidApiKey);

        await client.DisposeAsync();
        var exception = await Record.ExceptionAsync(async () => await client.DisposeAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task SubmitScoreAsync_AfterDisposeAsync_ThrowsObjectDisposedException()
    {
        var client = new AscndClient(ValidApiKey);
        await client.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.SubmitScoreAsync("leaderboard", "player", 100));
    }

    [Fact]
    public async Task Dispose_AfterDisposeAsync_DoesNotThrow()
    {
        var client = new AscndClient(ValidApiKey);

        await client.DisposeAsync();
        var exception = Record.Exception(() => client.Dispose());

        Assert.Null(exception);
    }
}
