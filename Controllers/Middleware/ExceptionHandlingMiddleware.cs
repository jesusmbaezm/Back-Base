using Services.DTOs.Shared;
using Services.Exceptions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Controllers.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string message;
            var errors = new List<string>();

            switch (exception)
            {
                case BusinessException businessException:
                    status = businessException.StatusCode;
                    message = businessException.Message;
                    break;

                case KeyNotFoundException:
                    status = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;

                case ArgumentException:
                    status = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    if (exception.InnerException is not null)
                        errors.Add(exception.InnerException.Message);
                    break;

                default:
                    status = HttpStatusCode.InternalServerError;
                    message = "Ocurrió un error interno en el servidor.";
                    errors.Add(GetFullExceptionMessage(exception));
                    break;
            }

            var response = ApiResponse<object>.Failure(message, errors);

            var payload = JsonSerializer.Serialize(response, _jsonOptions);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(payload);
        }

        private static string GetFullExceptionMessage(Exception exception)
        {
            var sb = new StringBuilder();
            var current = exception;
            while (current is not null)
            {
                sb.Append(current.Message);
                current = current.InnerException;
                if (current is not null) sb.Append(" → ");
            }
            return sb.ToString();
        }
    }
}