using Microsoft.AspNetCore.Mvc;
using ServiceA.ContentApi.Exceptions;
using System.Text.Json;

namespace ServiceA.ContentApi
{ 
    /// <summary>
    /// Custom exception middleware that catches all unhandled exceptions,
    /// logs them, and returns a structured RFC 7807 ProblemDetails response.
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
                _logger.LogError(ex, "Unhandled exception caught by ExceptionMiddleware: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                NotFoundException   => (StatusCodes.Status404NotFound, "Resource Not Found"),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
                _                   => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            var problemDetails = new ProblemDetails
            {
                Status      = statusCode,
                Title       = title,
                Detail      = exception.Message,
                Instance    = context.Request.Path
            };

            context.Response.StatusCode     = statusCode;
            context.Response.ContentType    = "application/prolem+json";

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}
