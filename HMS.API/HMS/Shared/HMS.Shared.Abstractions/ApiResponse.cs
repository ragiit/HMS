using System.Text.Json;
using System.Text.Json.Serialization;

namespace HMS.Shared.Abstractions;

/// <summary>
/// Format response API standar selaras dengan SDD 03-API-Design.
/// {
///   "success": true,
///   "data": { ... },
///   "message": "Success message",
///   "errors": null
/// }
/// </summary>
public sealed class ApiResponse
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<ApiError>? Errors { get; set; }

    public static ApiResponse Ok(object? data = null, string message = "Success") =>
        new() { Success = true, Data = data, Message = message, Errors = null };

    public static ApiResponse Fail(string message, IReadOnlyList<ApiError>? errors = null) =>
        new() { Success = false, Data = null, Message = message, Errors = errors };
}

/// <summary>
/// Response API generik yang membawa nilai <typeparamref name="T"/> di bagian <c>data</c>.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<ApiError>? Errors { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message, Errors = null };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ApiError>? errors = null) =>
        new() { Success = false, Data = default, Message = message, Errors = errors };

    /// <summary>
    /// Mengubah ApiResponse&lt;T&gt; menjadi ApiResponse polos (tanpa tipe).
    /// </summary>
    public ApiResponse ToNonGeneric() =>
        new() { Success = Success, Data = Data, Message = Message, Errors = Errors };
}

/// <summary>
/// Error per-field untuk respon validasi, selaras dengan format SDD:
/// { "field": "FirstName", "message": "First name is required" }
/// </summary>
public sealed record ApiError
{
    public string Field { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public ApiError() { }

    public ApiError(string field, string message)
    {
        Field = field;
        Message = message;
    }

    public static ApiError For(string field, string message) => new(field, message);
}

/// <summary>
/// Serializer options JSON konsisten yang dipakai antar service.
/// </summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}