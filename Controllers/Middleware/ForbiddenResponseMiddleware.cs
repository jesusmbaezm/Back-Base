using Services.DTOs.Shared;
using System.Text.Json;

namespace Controllers.Middleware
{
    public class ForbiddenResponseMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status403Forbidden && !context.Response.HasStarted && context.Response.ContentLength is null or 0)
            {
                var response = ApiResponse<object>.Failure(
                    "No tienes permisos para realizar esta acción.");

                context.Response.ContentType = "application/json";
                var payload = JsonSerializer.Serialize(response, _jsonOptions);
                await context.Response.WriteAsync(payload);
            }

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized && !context.Response.HasStarted && context.Response.ContentLength is null or 0)
            {
                var response = ApiResponse<object>.Failure(
                    "No estás autenticado. Inicia sesión para continuar.");

                context.Response.ContentType = "application/json";
                var payload = JsonSerializer.Serialize(response, _jsonOptions);
                await context.Response.WriteAsync(payload);
            }
        }
    }
}