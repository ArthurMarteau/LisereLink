using Lisere.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Lisere.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            BusinessException => (StatusCodes.Status400BadRequest, "Requête invalide"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Ressource introuvable"),
            _ => (StatusCodes.Status500InternalServerError, "Une erreur interne est survenue")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception occurred");

        var detail = statusCode == StatusCodes.Status500InternalServerError && !_env.IsDevelopment()
            ? "Une erreur interne est survenue."
            : exception.Message;

        var problemDetails = new ProblemDetails
        {
            Type = $"https://api.lisere.app/errors/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
