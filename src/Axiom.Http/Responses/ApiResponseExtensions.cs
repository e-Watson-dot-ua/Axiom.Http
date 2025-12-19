using System.Net;

namespace Axiom.Http.Responses;

/// <summary>
/// Convenience helpers for working with <see cref="ApiResponse"/>
/// and <see cref="ApiResponse{TData}"/>.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Returns <c>true</c> when the response represents a failure.
    /// </summary>
    public static bool IsFailure(this ApiResponse response) => !response.Success;

    /// <summary>
    /// Returns <c>true</c> when the response represents a failure.
    /// </summary>
    public static bool IsFailure<TData>(this ApiResponse<TData> response) => !response.Success;

    /// <summary>
    /// Adds/overwrites a metadata entry and returns a new response instance.
    /// </summary>
    public static ApiResponse WithMetadata(this ApiResponse response, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return response.With(metadata: CreateMetadata(response.Metadata, key, value));
    }

    /// <summary>
    /// Adds/overwrites a metadata entry and returns a new response instance.
    /// </summary>
    public static ApiResponse<TData> WithMetadata<TData>(this ApiResponse<TData> response, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return response.With(metadata: CreateMetadata(response.Metadata, key, value));
    }

    /// <summary>
    /// Attempts to read a strongly-typed value from response metadata.
    /// </summary>
    public static bool TryGetMetadata<T>(this ApiResponse response, string key, out T? value)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return TryGetMetadataCore(response.Metadata, key, out value);
    }

    /// <summary>
    /// Attempts to read a strongly-typed value from response metadata.
    /// </summary>
    public static bool TryGetMetadata<TData, T>(this ApiResponse<TData> response, string key, out T? value)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return TryGetMetadataCore(response.Metadata, key, out value);
    }

    /// <summary>
    /// Maps a successful API response to another data type.
    /// If the mapper returns <c>null</c>, the mapped response will have <c>Data = default</c>.
    /// </summary>
    public static ApiResponse<TOut> Map<TIn, TOut>(this ApiResponse<TIn> response, Func<TIn?, TOut?> mapper)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(mapper);

        if (!response.Success)
        {
            return MapFailure<TOut, TIn>(response);
        }

        var mapped = mapper(response.Data);

        // ApiResponse<T>.Ok requires a non-null argument; fall back to Ok(default!) and then overwrite.
        var ok = mapped is null
            ? ApiResponse<TOut>.Ok(default!, response.CorrelationId, response.Metadata).With(data: default)
            : ApiResponse<TOut>.Ok(mapped, response.CorrelationId, response.Metadata);

        return ok.With(timestamp: response.Timestamp);
    }

    /// <summary>
    /// A small representation compatible with RFC 7807-style error payloads.
    /// (Avoids taking a hard dependency on ASP.NET Core packages.)
    /// </summary>
    public sealed record ProblemDetails(
        string? Title,
        string? Detail,
        int? Status,
        string? Type = null,
        string? Instance = null,
        IDictionary<string, object?>? Extensions = null);

    /// <summary>
    /// Converts a failed response into a minimal RFC 7807-style object.
    /// </summary>
    public static ProblemDetails ToProblemDetails(this ApiResponse response, int? status = null, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(response);

        // If the caller didn't provide a status, infer a reasonable default.
        status ??= response.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest;

        var extensions = new Dictionary<string, object?>
        {
            ["success"] = response.Success,
            ["correlationId"] = response.CorrelationId,
            ["error"] = response.PrimaryError,
            ["errors"] = response.Errors,
            ["timestamp"] = response.Timestamp
        };

        if (response.Metadata is not null)
        {
            extensions["metadata"] = response.Metadata;
        }

        return new ProblemDetails(
            Title: title,
            Detail: null,
            Status: status,
            Extensions: extensions);
    }

    private static Dictionary<string, object> CreateMetadata(Dictionary<string, object>? existing, string key, object value)
    {
        var metadata = existing is null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>(existing);

        metadata[key] = value;
        return metadata;
    }

    private static bool TryGetMetadataCore<T>(Dictionary<string, object>? metadata, string key, out T? value)
    {
        value = default;

        if (metadata is null || !metadata.TryGetValue(key, out var raw))
        {
            return false;
        }

        if (raw is T t)
        {
            value = t;
            return true;
        }

        return false;
    }

    private static ApiResponse<TOut> MapFailure<TOut, TIn>(ApiResponse<TIn> response)
    {
        // Prefer the error list; fall back to a single error if present.
        if (response.Errors.Count > 0)
        {
            return ApiResponse<TOut>.Fail(response.Errors, response.CorrelationId)
                .With(metadata: response.Metadata, timestamp: response.Timestamp);
        }

        if (response.Error is not null)
        {
            return ApiResponse<TOut>.Fail(response.Error, response.CorrelationId)
                .With(metadata: response.Metadata, timestamp: response.Timestamp);
        }

        // Worst case: failed response without error info (possible through deserialization).
        return ApiResponse<TOut>.FailureWithoutError(
            correlationId: response.CorrelationId,
            metadata: response.Metadata,
            timestamp: response.Timestamp);
    }
}
