using Microsoft.AspNetCore.Mvc;
using Services.Constants;
using Services.DTOs.Parameters;
using Services.DTOs.Shared;
using Services.Services.Interfaces;
using Controllers.Attributes;
using System.Security.Claims;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/parameters")]
    public class ParametersController : BaseController
    {
        private readonly IParameterService _parametersService;

        public ParametersController(IParameterService parametersService)
        {
            _parametersService = parametersService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpGet]
        [HasPermission(Permissions.Parameter.Read)]
        public async Task<IActionResult> GetAll()
        {
            var parameters = await _parametersService.GetAllAsync();
            return OkResponse(parameters);
        }

        [HttpGet("paged")]
        [HasPermission(Permissions.Parameter.Read)]
        public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
        {
            var result = await _parametersService.GetPagedAsync(request);
            return OkResponse(result);
        }

        [HttpGet("{id}")]
        [HasPermission(Permissions.Parameter.Read)]
        public async Task<IActionResult> GetById(int id)
        {
            var parameter = await _parametersService.GetByIdAsync(id);
            if (parameter is null)
                return NotFoundResponse($"Parámetro con Id {id} no encontrado");
            return OkResponse(parameter);
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.Parameter.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateParameterDto dto)
        {
            var updated = await _parametersService.UpdateAsync(id, dto, GetUserId());
            if (updated is null)
                return NotFoundResponse($"Parámetro con Id {id} no encontrado");
            return OkResponse(updated);
        }


    }
}
