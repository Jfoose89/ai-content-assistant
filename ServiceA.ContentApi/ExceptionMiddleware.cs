using Microsoft.AspNetCore.Mvc;
using ServiceA.ContentApi.Exceptions;
using System.Text.Json;

namespace ServiceA.ContentApi
{ 
    /// <summary>
    /// Custom exception middleware that catches all unhandled exceptions,
    /// logs them, and returns a structured RFC 7807 ProblemDetails response.
    /// Never leaks API keys, internal service URLs, or reqeust headers.
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
            var (statusCode, title, detail) = exception switch
            {
                NotFoundException   => 
                    (StatusCodes.Status404NotFound, 
                    "Resource Not Found",
                    exception.Message),

                ValidationException => 
                    (StatusCodes.Status400BadRequest, 
                    "Validation Failed",
                    exception.Message),
                
                HttpRequestException hre when hre.StatusCode.HasValue =>
                    ((int)hre.StatusCode.Value switch
                    {
                        401 or 403 => StatusCodes.Status502BadGateway,
                        429        => StatusCodes.Status429TooManyRequests,
                        >= 500     => StatusCodes.Status502BadGateway,
                        _          => StatusCodes.Status502BadGateway
                    },
                    "AI Service Error",
                    "The AI service returned an error. Please try again later."),

                TaskCanceledException =>
                    (StatusCodes.Status504GatewayTimeout,
                    "AI Service Timeout",
                    "The request to the AI service timed out. Please try again"),

                _ =>
                    (StatusCodes.Status500InternalServerError,
                    "InternalServerError",
                    "An unexpected error occurred.")

            };

            var problemDetails = new ProblemDetails
            {
                Status      = statusCode,
                Title       = title,
                Detail      = detail,
                Instance    = context.Request.Path
            };

            context.Response.StatusCode     = statusCode;
            context.Response.ContentType    = "application/problem+json";

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}
