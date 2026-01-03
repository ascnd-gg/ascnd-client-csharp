using System;
using Microsoft.Extensions.Logging;

namespace Ascnd.Client
{
    /// <summary>
    /// Configuration options for the Ascnd client.
    /// </summary>
    public class AscndClientOptions
    {
        /// <summary>
        /// The gRPC endpoint URL of the Ascnd API.
        /// Defaults to the production API endpoint.
        /// </summary>
        /// <example>https://api.ascnd.gg</example>
        public string BaseUrl { get; set; } = "https://api.ascnd.gg";

        /// <summary>
        /// Your Ascnd API key.
        /// Obtain this from the Ascnd dashboard at https://dashboard.ascnd.gg.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Optional timeout for gRPC requests in seconds.
        /// Defaults to 30 seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Optional logger for diagnostic output.
        /// When set, the client will log request/response information and errors.
        /// </summary>
        /// <example>
        /// <code>
        /// var options = new AscndClientOptions("api-key")
        /// {
        ///     Logger = loggerFactory.CreateLogger&lt;AscndClient&gt;()
        /// };
        /// </code>
        /// </example>
        public ILogger<AscndClient>? Logger { get; set; }

        /// <summary>
        /// Creates a new instance of AscndClientOptions.
        /// </summary>
        public AscndClientOptions()
        {
        }

        /// <summary>
        /// Creates a new instance of AscndClientOptions with the specified API key.
        /// </summary>
        /// <param name="apiKey">Your Ascnd API key.</param>
        public AscndClientOptions(string apiKey)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        /// <summary>
        /// Creates a new instance of AscndClientOptions with the specified API key and base URL.
        /// </summary>
        /// <param name="apiKey">Your Ascnd API key.</param>
        /// <param name="baseUrl">The gRPC endpoint URL of the Ascnd API.</param>
        public AscndClientOptions(string apiKey, string baseUrl)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        /// <summary>
        /// Validates the options and throws if they are invalid.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when required options are missing or invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new InvalidOperationException("ApiKey is required. Obtain one from https://dashboard.ascnd.gg");
            }

            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new InvalidOperationException("BaseUrl is required.");
            }

            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                throw new InvalidOperationException("BaseUrl must be a valid HTTP or HTTPS URL.");
            }

            if (TimeoutSeconds <= 0)
            {
                throw new InvalidOperationException("TimeoutSeconds must be greater than zero.");
            }
        }
    }
}
