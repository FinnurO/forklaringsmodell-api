using System.Net;
using System.Text.Json;
using Forklaringsmodell.Application.Exceptions;

namespace Forklaringsmodell.Api.Middleware;

/// <summary>
/// Mapper domene-exceptions til riktige HTTP-statuskoder:
///   - AppendOnlyViolationException -> 409 Conflict (regel 3.1/3.4)
///   - AppValidationException       -> 400 Bad Request (FluentValidation-feil)
///   - NotFoundException            -> 404 Not Found
/// </summary>
public class ExceptionHandlingMiddleware
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
        catch (AppendOnlyViolationException ex)
        {
            _logger.LogWarning(ex, "Append-only-brudd");
            await WriteProblemAsync(context, HttpStatusCode.Conflict, "Append-only-brudd", ex.Message);
        }
        catch (AppValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/problem+json";
            var payload = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Validering feilet",
                status = 400,
                errors
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Ikke funnet", ex.Message);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode status, string title, string detail)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";
        var payload = new
        {
            type = $"https://tools.ietf.org/html/rfc9110#section-{(int)status}",
            title,
            status = (int)status,
            detail
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
