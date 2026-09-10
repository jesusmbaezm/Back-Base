using Microsoft.AspNetCore.Mvc;
using Controllers.Attributes;
using Services.DTOs.Shared;
using Services.Constants;
using Services.Services.Interfaces;
using Services.DTOs.Roles;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [HasPermission(Permissions.Role.Read)]
        public async Task<IActionResult> GetAll()
        {
            return OkResponse(await _roleService.GetAllAsync());
        }

        /// <summary>
        /// Lightweight list for select/combo inputs. Accessible with users.read permission.
        /// </summary>
        [HttpGet("select")]
        [HasPermission(Permissions.User.Read)]
        public async Task<IActionResult> GetSelect()
        {
            return OkResponse(await _roleService.GetAllAsync());
        }

        [HttpGet("paged")]
        [HasPermission(Permissions.Role.Read)]
        public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
        {
            var result = await _roleService.GetPagedAsync(request);
            return OkResponse(result);
        }

        [HttpGet("{id}")]
        [HasPermission(Permissions.Role.Read)]
        public async Task<IActionResult> GetById(int id)
        {
            return OkResponse(await _roleService.GetByIdAsync(id));
        }

        [HttpGet("permissions")]
        [HasPermission(Permissions.Role.Read)]
        public async Task<IActionResult> GetPermissions()
        {
            return OkResponse(await _roleService.GetPermissionsAsync());
        }

        [HttpPost]
        [HasPermission(Permissions.Role.Create)]
        public async Task<IActionResult> Create([FromBody] RoleRequestDto request)
        {
            var created = await _roleService.CreateAsync(request);
            return CreatedResponse(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.Role.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] RoleRequestDto request)
        {
            return OkResponse(await _roleService.UpdateAsync(id, request));
        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.Role.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            await _roleService.DeleteAsync(id);
            return SuccessEmptyResponse("Rol eliminado correctamente.");
        }
    }
}
