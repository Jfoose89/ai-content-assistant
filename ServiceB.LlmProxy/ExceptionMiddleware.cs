using Microsoft.AspNetCore.Mvc;
using ServiceB.LlmProxy.Services;
using System.Text.Json;

namespace ServiceB.LlmProxy;

/// <summary>
/// Custom exception middleware for Service B.
/// Catches HuggingFaceException and generic exceptions, returning structured
/// RFC 7807 ProblemDetails without leaking secrets or headers
/// </summary>

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            // Log the exception for debugging - never log headers or API keys
            _logger.LogError(ex, "Unhandled exception in ServiceB: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            HuggingFaceException hfe => (
                hfe.StatusCode,
                "AI Service Error",
                hfe.Message),
            _ => (
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "An unexpected error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}