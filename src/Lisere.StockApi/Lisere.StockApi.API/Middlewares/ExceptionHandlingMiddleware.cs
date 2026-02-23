using Lisere.StockApi.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Lisere.StockApi.API.Middlewares;

/// <summary>
/// Middleware global de gestion des erreurs — retourne ProblemDetails (RFC 7807).
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
        catch (StockException ex)
        {
            _logger.LogWarning(ex, "StockException: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest,
                "https://api.lisere.app/errors/stock-error",
                "Erreur de stock",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemDetailsAsync(context, HttpStatusCode.InternalServerError,
                "https://api.lisere.app/errors/internal-error",
                "Erreur interne du serveur",
                "Une erreur inattendue s'est produite.");
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string type,
        string title,
        string detail)
    {
        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
