using System.Net;
using System.Text.Json;
using TL.VerticalSlice.Template.Application.Common.Exceptions;
using TL.VerticalSlice.Template.Application.Common.Models;
using ValidationException = TL.VerticalSlice.Template.Application.Common.Exceptions.ValidationException;

namespace TL.VerticalSlice.Template.API.Middleware;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExceÃ§Ã£o nÃ£o tratada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            NotFoundException notFound => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Falha(notFound.Message)
            ),
            ValidationException validation => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Falha(
                    "Erros de validaÃ§Ã£o encontrados.",
                    validation.Errors.SelectMany(e => e.Value)
                )
            ),
            BusinessException business => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Falha(business.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Falha("Ocorreu um erro interno no servidor. Tente novamente mais tarde.")
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

