using Xunit;

namespace Ascnd.Client.Tests;

public class AscndClientOptionsTests
{
    [Fact]
    public void Constructor_WithApiKey_SetsApiKey()
    {
        var options = new AscndClientOptions("test-api-key");

        Assert.Equal("test-api-key", options.ApiKey);
        Assert.Equal("https://api.ascnd.gg", options.BaseUrl);
        Assert.Equal(30, options.TimeoutSeconds);
    }

    [Fact]
    public void Constructor_WithApiKeyAndBaseUrl_SetsBothProperties()
    {
        var options = new AscndClientOptions("test-api-key", "https://custom.api.com");

        Assert.Equal("test-api-key", options.ApiKey);
        Assert.Equal("https://custom.api.com", options.BaseUrl);
    }

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AscndClientOptions(null!));
    }

    [Fact]
    public void Constructor_WithNullBaseUrl_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AscndClientOptions("api-key", null!));
    }

    [Fact]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        var options = new AscndClientOptions("test-api-key");

        var exception = Record.Exception(() => options.Validate());

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrWhitespaceApiKey_ThrowsInvalidOperationException(string? apiKey)
    {
        var options = new AscndClientOptions { ApiKey = apiKey! };

        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());

        Assert.Contains("ApiKey is required", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyOrWhitespaceBaseUrl_ThrowsInvalidOperationException(string baseUrl)
    {
        var options = new AscndClientOptions("api-key") { BaseUrl = baseUrl };

        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());

        Assert.Contains("BaseUrl is required", exception.Message);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://invalid-scheme.com")]
    [InlineData("file:///local/path")]
    public void Validate_WithInvalidBaseUrl_ThrowsInvalidOperationException(string baseUrl)
    {
        var options = new AscndClientOptions("api-key") { BaseUrl = baseUrl };

        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());

        Assert.Contains("BaseUrl must be a valid HTTP or HTTPS URL", exception.Message);
    }

    [Theory]
    [InlineData("http://api.example.com")]
    [InlineData("https://api.example.com")]
    [InlineData("https://api.example.com:8080")]
    public void Validate_WithValidHttpOrHttpsBaseUrl_DoesNotThrow(string baseUrl)
    {
        var options = new AscndClientOptions("api-key") { BaseUrl = baseUrl };

        var exception = Record.Exception(() => options.Validate());

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithNonPositiveTimeout_ThrowsInvalidOperationException(int timeout)
    {
        var options = new AscndClientOptions("api-key") { TimeoutSeconds = timeout };

        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());

        Assert.Contains("TimeoutSeconds must be greater than zero", exception.Message);
    }

    [Fact]
    public void Validate_WithPositiveTimeout_DoesNotThrow()
    {
        var options = new AscndClientOptions("api-key") { TimeoutSeconds = 60 };

        var exception = Record.Exception(() => options.Validate());

        Assert.Null(exception);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var options = new AscndClientOptions();

        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal("https://api.ascnd.gg", options.BaseUrl);
        Assert.Equal(30, options.TimeoutSeconds);
    }
}
