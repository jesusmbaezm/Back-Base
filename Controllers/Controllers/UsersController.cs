using Microsoft.AspNetCore.Mvc;
using Services.Constants;
using Services.DTOs.Users;
using Services.Services.Interfaces;
using Controllers.Attributes;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseController
    {
        private readonly IUserManagementService _userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        [HasPermission(Permissions.User.Read)]
        public async Task<IActionResult> GetAll()
        {
            return OkResponse(await _userManagementService.GetAllAsync());
        }

        [HttpGet("paged")]
        [HasPermission(Permissions.User.Read)]
        public async Task<IActionResult> GetPaged([FromQuery] UserPagedRequestDto request)
        {
            var result = await _userManagementService.GetPagedAsync(request);
            return OkResponse(result);
        }

        [HttpGet("{id}")]
        [HasPermission(Permissions.User.Read)]
        public async Task<IActionResult> GetById(int id)
        {
            return OkResponse(await _userManagementService.GetByIdAsync(id));
        }

        [HttpPost]
        [HasPermission(Permissions.User.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUserDto request)
        {
            var created = await _userManagementService.CreateAsync(request);
            return CreatedResponse(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.User.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto request)
        {
            return OkResponse(await _userManagementService.UpdateAsync(id, request));
        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.User.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            await _userManagementService.DeleteAsync(id);
            return SuccessEmptyResponse("Usuario eliminado correctamente.");
        }

        [HttpPost("{id}/reset-password")]
        [HasPermission(Permissions.User.ResetPassword)]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetUserPasswordDto request)
        {
            await _userManagementService.ResetPasswordAsync(id, request);
            return NoContentResponse();
        }
    }
}
