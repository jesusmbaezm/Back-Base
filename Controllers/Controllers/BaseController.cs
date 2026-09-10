using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Shared;

namespace Controllers.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult OkResponse<T>(T data, string message = "") =>
            Ok(ApiResponse<T>.Success(data, message));

        protected IActionResult CreatedResponse<T>(string actionName, object routeValues, T data, string message = "") =>
            CreatedAtAction(actionName, routeValues, ApiResponse<T>.Success(data, message));

        protected IActionResult NotFoundResponse(string message) =>
            NotFound(ApiResponse.Failure(message));

        protected IActionResult BadRequestResponse(string message, List<string>? errors = null) =>
            BadRequest(ApiResponse<object>.Failure(message, errors));

        protected IActionResult UnauthorizedResponse(string message, List<string>? errors = null) =>
            Unauthorized(ApiResponse<object>.Failure(message, errors));

        protected IActionResult SuccessEmptyResponse(string message = "") =>
            Ok(ApiResponse.SuccessEmpty(message));

        protected IActionResult NoContentResponse() =>
            NoContent();
    }
}
