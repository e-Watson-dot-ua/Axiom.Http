using Axiom.Application.Results;
using Axiom.Primitives.Guards;

namespace Axiom.Http.Responses;

/// <summary>
/// Represents a standardized API response wrapper.
/// </summary>
/// <typeparam name="TData">The type of data in the response.</typeparam>
public sealed class ApiResponse<TData>
{
    /// <summary>
    /// Gets a value indicating whether the request was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the response data.
    /// </summary>
    public TData? Data { get; init; }

    /// <summary>
    /// Gets the error information if the request failed.
    /// </summary>
    public Error? Error { get; init; }

    /// <summary>
    /// Gets additional errors that occurred.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; init; } = Array.Empty<Error>();

    /// <summary>
    /// Gets the primary error (first error) when the request failed.
    /// </summary>
    public Error? PrimaryError => Errors.Count > 0 ? Errors[0] : Error;

    /// <summary>
    /// Gets the timestamp when the response was generated.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the correlation ID for request tracking.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets additional metadata about the response.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{TData}"/> class.
    /// </summary>
    private ApiResponse() { }

    /// <summary>
    /// Creates a successful API response with data.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A successful <see cref="ApiResponse{TData}"/>.</returns>
    public static ApiResponse<TData> Ok(TData data, string? correlationId = null,
        Dictionary<string, object>? metadata = null) =>
        new()
        {
            Success = true,
            Data = data,
            CorrelationId = correlationId,
            Metadata = metadata
        };

    /// <summary>
    /// Creates a failed API response with a single error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <returns>A failed <see cref="ApiResponse{TData}"/>.</returns>
    public static ApiResponse<TData> Fail(Error error, string? correlationId = null) =>
        new()
        {
            Success = false,
            Error = error,
            CorrelationId = correlationId
        };

    /// <summary>
    /// Creates a failed API response with multiple errors.
    /// </summary>
    /// <param name="errors">The errors that occurred.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <returns>A failed <see cref="ApiResponse{TData}"/>.</returns>
    public static ApiResponse<TData> Fail(IReadOnlyList<Error> errors,
        string? correlationId = null)
    {
        if (errors == null || errors.Count == 0)
        {
            throw new ArgumentException("At least one error must be provided.", nameof(errors));
        }

        return new ApiResponse<TData>
        {
            Success = false,
            Error = errors[0],
            Errors = errors,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Creates an API response from a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <returns>An <see cref="ApiResponse{TData}"/>.</returns>
    public static ApiResponse<TData> FromResult(Result<TData> result,
        string? correlationId = null)
    {
        Guard.Against.Null(result, nameof(result));

        if (result.IsSuccess)
        {
            return Ok(result.Value, correlationId);
        }

        return result.Errors.Count > 0
            ? Fail(result.Errors, correlationId)
            : Fail(result.Error, correlationId);
    }

    internal ApiResponse<TData> With(
        bool? success = null,
        TData? data = default,
        Error? error = null,
        IReadOnlyList<Error>? errors = null,
        DateTimeOffset? timestamp = null,
        string? correlationId = null,
        Dictionary<string, object>? metadata = null)
    {
        return new ApiResponse<TData>
        {
            Success = success ?? Success,
            Data = data is not null || !Equals(data, default(TData)) ? data : Data,
            Error = error ?? Error,
            Errors = errors ?? Errors,
            Timestamp = timestamp ?? Timestamp,
            CorrelationId = correlationId ?? CorrelationId,
            Metadata = metadata ?? Metadata
        };
    }

    internal static ApiResponse<TData> FailureWithoutError(string? correlationId = null, Dictionary<string, object>? metadata = null,
        DateTimeOffset? timestamp = null)
    {
        return new ApiResponse<TData>
        {
            Success = false,
            Errors = Array.Empty<Error>(),
            CorrelationId = correlationId,
            Metadata = metadata,
            Timestamp = timestamp ?? DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Represents a standardized API response wrapper without data.
/// </summary>
public sealed class ApiResponse
{
    /// <summary>
    /// Gets a value indicating whether the request was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the error information if the request failed.
    /// </summary>
    public Error? Error { get; init; }

    /// <summary>
    /// Gets additional errors that occurred.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; init; } = Array.Empty<Error>();

    /// <summary>
    /// Gets the primary error (first error) when the request failed.
    /// </summary>
    public Error? PrimaryError => Errors.Count > 0 ? Errors[0] : Error;

    /// <summary>
    /// Gets the timestamp when the response was generated.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the correlation ID for request tracking.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets additional metadata about the response.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse"/> class.
    /// </summary>
    private ApiResponse() { }

    /// <summary>
    /// Creates a successful API response.
    /// </summary>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A successful <see cref="ApiResponse"/>.</returns>
    public static ApiResponse Ok(string? correlationId = null,
        Dictionary<string, object>? metadata = null) =>
        new()
        {
            Success = true,
            CorrelationId = correlationId,
            Metadata = metadata
        };

    /// <summary>
    /// Creates a failed API response with a single error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <returns>A failed <see cref="ApiResponse"/>.</returns>
    public static ApiResponse Fail(Error error, string? correlationId = null) =>
        new()
        {
            Success = false,
            Error = error,
            CorrelationId = correlationId
        };

    /// <summary>
    /// Creates a failed API response with multiple errors.
    /// </summary>
    /// <param name="errors">The errors that occurred.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <returns>A failed <see cref="ApiResponse"/>.</returns>
    public static ApiResponse Fail(IReadOnlyList<Error> errors, string? correlationId = null)
    {
        if (errors == null || errors.Count == 0)
        {
            throw new ArgumentException("At least one error must be provided.", nameof(errors));
        }

        return new ApiResponse
        {
            Success = false,
            Error = errors[0],
            Errors = errors,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Creates an API response from a <see cref="Result"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <returns>An <see cref="ApiResponse"/>.</returns>
    public static ApiResponse FromResult(Result result, string? correlationId = null)
    {
        Guard.Against.Null(result, nameof(result));

        if (result.IsSuccess)
        {
            return Ok(correlationId);
        }

        return result.Errors.Count > 0
            ? Fail(result.Errors, correlationId)
            : Fail(result.Error, correlationId);
    }

    internal ApiResponse With(
        bool? success = null,
        Error? error = null,
        IReadOnlyList<Error>? errors = null,
        DateTimeOffset? timestamp = null,
        string? correlationId = null,
        Dictionary<string, object>? metadata = null)
    {
        return new ApiResponse
        {
            Success = success ?? Success,
            Error = error ?? Error,
            Errors = errors ?? Errors,
            Timestamp = timestamp ?? Timestamp,
            CorrelationId = correlationId ?? CorrelationId,
            Metadata = metadata ?? Metadata
        };
    }
}
