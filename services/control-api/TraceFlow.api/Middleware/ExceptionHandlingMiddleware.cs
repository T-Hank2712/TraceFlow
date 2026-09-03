using System.Net;
using System.Text.Json;
using FluentValidation;
using TraceFlow.Api.Application.Common.Exceptions;

namespace TraceFlow.Api.Middleware
{
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
            catch(ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation Failed.");
                await HandleValidationExceptionAsync(context, ex);
            }
            catch(NotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found.");
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception.");

                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.");
            }
        }
        private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            var response = new {status = (int)statusCode, message};
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "pplication/json";
            var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                gr => gr.Key,
                gr => gr.Select(error => error.ErrorMessage).ToArray()
            );
            var response = new
            {
                status = StatusCodes.Status400BadRequest,
                message = "One or more validation errors occurred.",
                errors
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}