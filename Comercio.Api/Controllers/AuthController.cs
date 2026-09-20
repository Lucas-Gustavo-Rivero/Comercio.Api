using Comercio.Api.DTOs.User;
using Comercio.Api.Extensions;
using Comercio.Api.Filters;
using Comercio.Api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ServiceFilter(typeof(ValidationFilter<LoginDto>))]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var resultado = await _authService.LoginAsync(dto);

            if(resultado.IsSuccess)
            {
                return Ok(resultado.Data);
            }

            return resultado.ToErrorResponse(this);
        }

        [HttpPost("register")]
        [ServiceFilter(typeof(ValidationFilter<RegisterDto>))]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            var resultado = await _authService.RegisterAsync(dto);

            if(resultado.IsSuccess)
            {
                return Ok(resultado.Data);
            }

            return resultado.ToErrorResponse(this);
        }
    }
}
