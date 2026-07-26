using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TaskManagement.Application.Exceptions;
using ValidationException = TaskManagement.Application.Exceptions.ValidationException;

namespace TaskManagement.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            NotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = notFound.Message
            },
            ForbiddenAccessException forbidden => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = forbidden.Message
            },
            UnauthorizedException unauthorized => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = unauthorized.Message
            },
            DuplicateEntityException duplicate => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = duplicate.Message
            },
            ValidationException validation => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = validation.Message
            },
            FluentValidation.ValidationException fluentValidation => CreateFluentValidationProblemDetails(fluentValidation),
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            }
        };

        problemDetails.Instance = context.Request.Path;
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private static ProblemDetails CreateFluentValidationProblemDetails(FluentValidation.ValidationException ex)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var failure in ex.Errors)
        {
            var propertyName = string.IsNullOrEmpty(failure.PropertyName) ? "general" : failure.PropertyName;

            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = Array.Empty<string>();
            }

            errors[propertyName] = errors[propertyName].Append(failure.ErrorMessage).ToArray();
        }

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred."
        };

        return problem;
    }
}
