using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ascnd.Client.Grpc;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

#if !NETSTANDARD2_0
using Grpc.Net.Client;
#endif

namespace Ascnd.Client
{
    /// <summary>
    /// Client for the Ascnd leaderboard API using gRPC.
    /// Provides methods for submitting scores and retrieving leaderboard data.
    /// </summary>
    /// <example>
    /// <code>
    /// // Create client with your API key
    /// using var client = new AscndClient("your-api-key");
    ///
    /// // Submit a score
    /// var result = await client.SubmitScoreAsync("weekly-highscores", "player123", 42500);
    ///
    /// Console.WriteLine($"New rank: {result.Rank}");
    /// </code>
    /// </example>
#if !NETSTANDARD2_0
    public class AscndClient : IDisposable, IAsyncDisposable
#else
    public class AscndClient : IDisposable
#endif
    {
        private readonly AscndClientOptions _options;
        private readonly AscndService.AscndServiceClient _grpcClient;
        private readonly Metadata _authMetadata;
        private readonly ILogger<AscndClient>? _logger;
        private int _disposed;

#if NETSTANDARD2_0
        private readonly Channel _channel;
#else
        private readonly GrpcChannel _channel;
#endif

        /// <summary>
        /// Creates a new AscndClient with the specified API key.
        /// Uses the default production API endpoint.
        /// </summary>
        /// <param name="apiKey">Your Ascnd API key from https://dashboard.ascnd.gg</param>
        /// <exception cref="ArgumentNullException">Thrown when apiKey is null.</exception>
        public AscndClient(string apiKey) : this(new AscndClientOptions(apiKey))
        {
        }

        /// <summary>
        /// Creates a new AscndClient with the specified options.
        /// </summary>
        /// <param name="options">Client configuration options.</param>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when options are invalid.</exception>
        public AscndClient(AscndClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();

            _logger = _options.Logger;

            // Set up authentication metadata
            _authMetadata = new Metadata
            {
                { "x-api-key", _options.ApiKey }
            };

#if NETSTANDARD2_0
            // For .NET Standard 2.0 (Unity), use Grpc.Core
            var channelCredentials = _options.BaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? ChannelCredentials.SecureSsl
                : ChannelCredentials.Insecure;

            var uri = new Uri(_options.BaseUrl);
            var target = uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);

            _channel = new Channel(target, channelCredentials);
            _grpcClient = new AscndService.AscndServiceClient(_channel);
#else
            // For .NET 6.0+, use Grpc.Net.Client
            _channel = GrpcChannel.ForAddress(_options.BaseUrl);
            _grpcClient = new AscndService.AscndServiceClient(_channel);
#endif

            _logger?.LogInformation("AscndClient initialized with endpoint {BaseUrl}", _options.BaseUrl);
        }

        /// <summary>
        /// Submits a player's score to a leaderboard.
        /// </summary>
        /// <param name="request">The score submission request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result of the score submission, including the player's new rank.</returns>
        /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
        /// <exception cref="AscndApiException">Thrown when the API returns an error.</exception>
        /// <example>
        /// <code>
        /// var request = new SubmitScoreRequest
        /// {
        ///     LeaderboardId = "weekly-highscores",
        ///     PlayerId = "player123",
        ///     Score = 42500
        /// };
        /// var result = await client.SubmitScoreAsync(request);
        ///
        /// if (result.IsNewBest)
        /// {
        ///     Console.WriteLine("New personal best!");
        /// }
        /// </code>
        /// </example>
        public async Task<SubmitScoreResponse> SubmitScoreAsync(
            SubmitScoreRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ThrowIfDisposed();

            _logger?.LogDebug("SubmitScoreAsync called for leaderboard {LeaderboardId}, player {PlayerId}, score {Score}",
                request.LeaderboardId, request.PlayerId, request.Score);

            try
            {
                var callOptions = CreateCallOptions(cancellationToken);
                var response = await _grpcClient.SubmitScoreAsync(request, callOptions);

                _logger?.LogInformation("Score submitted successfully. Rank: {Rank}, IsNewBest: {IsNewBest}",
                    response.Rank, response.IsNewBest);

                return response;
            }
            catch (RpcException ex)
            {
                _logger?.LogError(ex, "gRPC error in SubmitScoreAsync: {StatusCode} - {Detail}",
                    ex.StatusCode, ex.Status.Detail);
                throw CreateApiException(ex);
            }
        }

        /// <summary>
        /// Retrieves leaderboard entries.
        /// </summary>
        /// <param name="request">The leaderboard request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The leaderboard entries and pagination information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
        /// <exception cref="AscndApiException">Thrown when the API returns an error.</exception>
        /// <example>
        /// <code>
        /// var request = new GetLeaderboardRequest
        /// {
        ///     LeaderboardId = "weekly-highscores",
        ///     Limit = 10
        /// };
        /// var leaderboard = await client.GetLeaderboardAsync(request);
        ///
        /// foreach (var entry in leaderboard.Entries)
        /// {
        ///     Console.WriteLine($"#{entry.Rank}: {entry.PlayerId} - {entry.Score}");
        /// }
        /// </code>
        /// </example>
        public async Task<GetLeaderboardResponse> GetLeaderboardAsync(
            GetLeaderboardRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ThrowIfDisposed();

            _logger?.LogDebug("GetLeaderboardAsync called for leaderboard {LeaderboardId}, limit {Limit}, cursor {Cursor}",
                request.LeaderboardId, request.Limit, request.Cursor);

            try
            {
                var callOptions = CreateCallOptions(cancellationToken);
                var response = await _grpcClient.GetLeaderboardAsync(request, callOptions);

                _logger?.LogInformation("Leaderboard retrieved. Entries: {EntryCount}, TotalEntries: {TotalEntries}",
                    response.Entries.Count, response.TotalEntries);

                return response;
            }
            catch (RpcException ex)
            {
                _logger?.LogError(ex, "gRPC error in GetLeaderboardAsync: {StatusCode} - {Detail}",
                    ex.StatusCode, ex.Status.Detail);
                throw CreateApiException(ex);
            }
        }

        /// <summary>
        /// Retrieves a specific player's rank on a leaderboard.
        /// </summary>
        /// <param name="request">The player rank request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The player's rank information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
        /// <exception cref="AscndApiException">Thrown when the API returns an error.</exception>
        /// <example>
        /// <code>
        /// var request = new GetPlayerRankRequest
        /// {
        ///     LeaderboardId = "weekly-highscores",
        ///     PlayerId = "player123"
        /// };
        /// var playerRank = await client.GetPlayerRankAsync(request);
        ///
        /// if (playerRank.HasRank)
        /// {
        ///     Console.WriteLine($"You are #{playerRank.Rank} ({playerRank.Percentile})");
        /// }
        /// </code>
        /// </example>
        public async Task<GetPlayerRankResponse> GetPlayerRankAsync(
            GetPlayerRankRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ThrowIfDisposed();

            _logger?.LogDebug("GetPlayerRankAsync called for leaderboard {LeaderboardId}, player {PlayerId}",
                request.LeaderboardId, request.PlayerId);

            try
            {
                var callOptions = CreateCallOptions(cancellationToken);
                var response = await _grpcClient.GetPlayerRankAsync(request, callOptions);

                if (response.HasRank)
                {
                    _logger?.LogInformation("Player rank retrieved. Rank: {Rank}, Score: {Score}",
                        response.Rank, response.Score);
                }
                else
                {
                    _logger?.LogInformation("Player has no rank on leaderboard {LeaderboardId}",
                        request.LeaderboardId);
                }

                return response;
            }
            catch (RpcException ex)
            {
                _logger?.LogError(ex, "gRPC error in GetPlayerRankAsync: {StatusCode} - {Detail}",
                    ex.StatusCode, ex.Status.Detail);
                throw CreateApiException(ex);
            }
        }

        /// <summary>
        /// Convenience method to submit a score with minimal parameters.
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID.</param>
        /// <param name="playerId">The player's unique identifier.</param>
        /// <param name="score">The score value.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result of the score submission.</returns>
        public Task<SubmitScoreResponse> SubmitScoreAsync(
            string leaderboardId,
            string playerId,
            long score,
            CancellationToken cancellationToken = default)
        {
            var request = new SubmitScoreRequest
            {
                LeaderboardId = leaderboardId,
                PlayerId = playerId,
                Score = score
            };
            return SubmitScoreAsync(request, cancellationToken);
        }

        /// <summary>
        /// Convenience method to submit a score with metadata.
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID.</param>
        /// <param name="playerId">The player's unique identifier.</param>
        /// <param name="score">The score value.</param>
        /// <param name="metadata">Optional metadata as bytes.</param>
        /// <param name="idempotencyKey">Optional idempotency key.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result of the score submission.</returns>
        public Task<SubmitScoreResponse> SubmitScoreAsync(
            string leaderboardId,
            string playerId,
            long score,
            byte[]? metadata,
            string? idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            var request = new SubmitScoreRequest
            {
                LeaderboardId = leaderboardId,
                PlayerId = playerId,
                Score = score
            };

            if (metadata != null)
            {
                request.Metadata = ByteString.CopyFrom(metadata);
            }

            if (idempotencyKey != null)
            {
                request.IdempotencyKey = idempotencyKey;
            }

            return SubmitScoreAsync(request, cancellationToken);
        }

        /// <summary>
        /// Convenience method to get a leaderboard with minimal parameters.
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID.</param>
        /// <param name="limit">Maximum number of entries to return (default: 10, max: 100).</param>
        /// <param name="cursor">Cursor for pagination (from previous response's NextCursor).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The leaderboard entries.</returns>
        public Task<GetLeaderboardResponse> GetLeaderboardAsync(
            string leaderboardId,
            int limit = 10,
            string? cursor = null,
            CancellationToken cancellationToken = default)
        {
            var request = new GetLeaderboardRequest
            {
                LeaderboardId = leaderboardId,
                Limit = limit
            };

            if (cursor != null)
            {
                request.Cursor = cursor;
            }

            return GetLeaderboardAsync(request, cancellationToken);
        }

        /// <summary>
        /// Convenience method to get a player's rank with minimal parameters.
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID.</param>
        /// <param name="playerId">The player's unique identifier.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The player's rank information.</returns>
        public Task<GetPlayerRankResponse> GetPlayerRankAsync(
            string leaderboardId,
            string playerId,
            CancellationToken cancellationToken = default)
        {
            var request = new GetPlayerRankRequest
            {
                LeaderboardId = leaderboardId,
                PlayerId = playerId
            };
            return GetPlayerRankAsync(request, cancellationToken);
        }

        private CallOptions CreateCallOptions(CancellationToken cancellationToken)
        {
            var deadline = DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds);
            return new CallOptions(headers: _authMetadata, deadline: deadline, cancellationToken: cancellationToken);
        }

        private static AscndApiException CreateApiException(RpcException ex)
        {
            var statusCode = MapGrpcStatusToHttpStatus(ex.StatusCode);
            return new AscndApiException(
                $"gRPC request failed: {ex.Status.Detail}",
                statusCode,
                ex.Status.Detail,
                ex.StatusCode,
                ex);
        }

        private static int MapGrpcStatusToHttpStatus(StatusCode grpcStatus)
        {
            return grpcStatus switch
            {
                StatusCode.OK => 200,
                StatusCode.Cancelled => 499,
                StatusCode.Unknown => 500,
                StatusCode.InvalidArgument => 400,
                StatusCode.DeadlineExceeded => 504,
                StatusCode.NotFound => 404,
                StatusCode.AlreadyExists => 409,
                StatusCode.PermissionDenied => 403,
                StatusCode.ResourceExhausted => 429,
                StatusCode.FailedPrecondition => 400,
                StatusCode.Aborted => 409,
                StatusCode.OutOfRange => 400,
                StatusCode.Unimplemented => 501,
                StatusCode.Internal => 500,
                StatusCode.Unavailable => 503,
                StatusCode.DataLoss => 500,
                StatusCode.Unauthenticated => 401,
                _ => 500
            };
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(AscndClient));
            }
        }

        /// <summary>
        /// Disposes the client and releases resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the client and releases resources.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _logger?.LogDebug("Disposing AscndClient");

                if (disposing)
                {
#if NETSTANDARD2_0
                    _channel.ShutdownAsync().Wait();
#else
                    _channel.Dispose();
#endif
                }

                _logger?.LogInformation("AscndClient disposed");
            }
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Asynchronously disposes the client and releases resources.
        /// </summary>
        /// <returns>A ValueTask representing the async dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _logger?.LogDebug("Disposing AscndClient asynchronously");
                await _channel.ShutdownAsync().ConfigureAwait(false);
                _logger?.LogInformation("AscndClient disposed");
            }
            GC.SuppressFinalize(this);
        }
#endif
    }

    /// <summary>
    /// Exception thrown when the Ascnd API returns an error.
    /// </summary>
    public class AscndApiException : Exception
    {
        /// <summary>
        /// The HTTP status code equivalent for the error.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// The error details from the API.
        /// </summary>
        public string ResponseBody { get; }

        /// <summary>
        /// The gRPC status code if available.
        /// </summary>
        public StatusCode? GrpcStatusCode { get; }

        /// <summary>
        /// Creates a new AscndApiException.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code equivalent.</param>
        /// <param name="responseBody">The error details.</param>
        public AscndApiException(string message, int statusCode, string responseBody)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            GrpcStatusCode = null;
        }

        /// <summary>
        /// Creates a new AscndApiException from a gRPC error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code equivalent.</param>
        /// <param name="responseBody">The error details.</param>
        /// <param name="grpcStatusCode">The gRPC status code.</param>
        /// <param name="innerException">The inner exception.</param>
        public AscndApiException(string message, int statusCode, string responseBody, StatusCode grpcStatusCode, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            GrpcStatusCode = grpcStatusCode;
        }
    }
}
