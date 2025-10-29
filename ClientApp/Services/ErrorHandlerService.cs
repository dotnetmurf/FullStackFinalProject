using System.Net;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Centralizes error handling and user-friendly message generation
/// </summary>
/// <remarks>
/// This service translates technical exceptions into user-friendly error messages
/// with actionable guidance. It provides consistent error handling across the application.
/// </remarks>
public class ErrorHandlerService
{
    private readonly ILogger<ErrorHandlerService> _logger;

    /// <summary>
    /// Initializes a new instance of the ErrorHandlerService
    /// </summary>
    /// <param name="logger">Logger for error tracking</param>
    public ErrorHandlerService(ILogger<ErrorHandlerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts an exception into a user-friendly error message
    /// </summary>
    /// <param name="ex">The exception to handle</param>
    /// <param name="context">Description of what the user was trying to do (e.g., "loading products")</param>
    /// <returns>A user-friendly error with actionable guidance</returns>
    public UserError HandleException(Exception ex, string context)
    {
        _logger.LogError(ex, "Error in context: {Context}", context);

        return ex switch
        {
            HttpRequestException httpEx => HandleHttpException(httpEx, context),
            TaskCanceledException => new UserError
            {
                Title = "Request Timeout",
                Message = "The operation took too long to complete.",
                ActionMessage = "Please try again. If the problem persists, check your internet connection.",
                Severity = ErrorSeverity.Warning,
                IsRetryable = true
            },
            InvalidOperationException invalidEx => new UserError
            {
                Title = "Invalid Operation",
                Message = invalidEx.Message,
                ActionMessage = "Please refresh the page and try again.",
                Severity = ErrorSeverity.Warning,
                IsRetryable = true
            },
            _ => new UserError
            {
                Title = "Unexpected Error",
                Message = "An unexpected error occurred.",
                ActionMessage = "Please try again later or contact support if the problem persists.",
                Severity = ErrorSeverity.Error,
                IsRetryable = false
            }
        };
    }

    /// <summary>
    /// Handles HTTP-specific exceptions with status code awareness
    /// </summary>
    private UserError HandleHttpException(HttpRequestException ex, string context)
    {
        if (ex.StatusCode == null)
        {
            // Network/connection error
            return new UserError
            {
                Title = "Connection Error",
                Message = "Unable to connect to the server.",
                ActionMessage = "Please check that:\n• The server is running (http://localhost:5132)\n• Your internet connection is active\n• No firewall is blocking the connection",
                Severity = ErrorSeverity.Error,
                IsRetryable = true
            };
        }

        return ex.StatusCode switch
        {
            HttpStatusCode.BadRequest => new UserError
            {
                Title = "Invalid Request",
                Message = "The submitted data was invalid.",
                ActionMessage = "Please check the form and correct any errors.",
                Severity = ErrorSeverity.Warning,
                IsRetryable = false
            },
            HttpStatusCode.Unauthorized => new UserError
            {
                Title = "Unauthorized",
                Message = "You are not authorized to perform this action.",
                ActionMessage = "Please log in and try again.",
                Severity = ErrorSeverity.Warning,
                IsRetryable = false
            },
            HttpStatusCode.Forbidden => new UserError
            {
                Title = "Access Denied",
                Message = "You don't have permission to access this resource.",
                ActionMessage = "Contact your administrator if you believe this is an error.",
                Severity = ErrorSeverity.Warning,
                IsRetryable = false
            },
            HttpStatusCode.NotFound => new UserError
            {
                Title = "Not Found",
                Message = GetNotFoundMessage(context),
                ActionMessage = "The item may have been deleted or the link may be incorrect.",
                Severity = ErrorSeverity.Info,
                IsRetryable = false
            },
            HttpStatusCode.Conflict => new UserError
            {
                Title = "Conflict",
                Message = "This operation conflicts with existing data.",
                ActionMessage = "A product with this name may already exist. Please use a different name.",
                Severity = ErrorSeverity.Warning,
                IsRetryable = false
            },
            HttpStatusCode.InternalServerError => new UserError
            {
                Title = "Server Error",
                Message = "The server encountered an error processing your request.",
                ActionMessage = "Please try again later. If the problem persists, contact support.",
                Severity = ErrorSeverity.Error,
                IsRetryable = true
            },
            HttpStatusCode.ServiceUnavailable => new UserError
            {
                Title = "Service Unavailable",
                Message = "The server is temporarily unavailable.",
                ActionMessage = "Please try again in a few moments.",
                Severity = ErrorSeverity.Warning,
                IsRetryable = true
            },
            _ => new UserError
            {
                Title = "Request Failed",
                Message = $"The server returned an error: {ex.StatusCode}",
                ActionMessage = "Please try again or contact support if the problem persists.",
                Severity = ErrorSeverity.Error,
                IsRetryable = true
            }
        };
    }

    /// <summary>
    /// Gets a context-specific message for 404 errors
    /// </summary>
    private string GetNotFoundMessage(string context)
    {
        return context.ToLower() switch
        {
            var c when c.Contains("product") => "The requested product was not found.",
            var c when c.Contains("category") => "The requested category was not found.",
            var c when c.Contains("load") => "The requested resource was not found.",
            _ => "The requested item was not found."
        };
    }
}
