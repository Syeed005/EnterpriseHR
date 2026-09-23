using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseHR.Api.ExceptionHandling {
    public class GlobalExceptionHandler : IExceptionHandler {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken) {
            var (statusCode, title, detail) = exception switch {
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", "You do not have access to this resource."),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
            };

            if (statusCode >= 500)
                _logger.LogError(exception, "Unhandled exception occurred.");
            else
                _logger.LogWarning(exception, "Request failed with status code {StatusCode}.", statusCode);

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails {
                Status = statusCode,
                Title = title,
                Detail = detail
            }, cancellationToken);

            return true;
        }
    }
}
