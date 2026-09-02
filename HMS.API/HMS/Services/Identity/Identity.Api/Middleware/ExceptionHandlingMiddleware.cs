using HMS.Shared.Abstractions;
using HMS.Shared.Abstractions.Exceptions;
using System.Net;

namespace HMS.Identity.Api.Middleware;

/// <summary>
/// Global exception handling: memetakan HmsException ke status HTTP yang sesuai dan
/// mengembalikan ApiResponse standar, selaras SDD 07 Cross-Cutting Error Handling.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HmsException ex)
        {
            _logger.LogWarning(ex, "Business exception: {Message}", ex.Message);
            await WriteErrorAsync(context, ex, MapStatus(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorAsync(
                context,
                new Exception("Terjadi kesalahan internal server."),
                HttpStatusCode.InternalServerError);
        }
    }

    private static HttpStatusCode MapStatus(HmsException ex) => ex switch
    {
        NotFoundException => HttpStatusCode.NotFound,
        ValidationException => HttpStatusCode.BadRequest,
        UnauthorizedException => HttpStatusCode.Unauthorized,
        PermissionDeniedException => HttpStatusCode.Forbidden,
        ConflictException or BusinessRuleViolationException => HttpStatusCode.Conflict,
        ConcurrencyException => HttpStatusCode.Conflict,
        _ => HttpStatusCode.BadRequest
    };

    private static async Task WriteErrorAsync(
        HttpContext context, Exception ex, HttpStatusCode statusCode)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        IReadOnlyList<ApiError>? errors = null;
        if (ex is ValidationException ve)
            errors = ve.Failures.Select(f => ApiError.For(f.Field, f.Message)).ToList();

        var response = ApiResponse.Fail(ex.Message, errors);
        await context.Response.WriteAsJsonAsync(response, JsonDefaults.Options);
    }
}