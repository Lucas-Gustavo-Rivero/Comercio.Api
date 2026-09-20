using Comercio.Api.DTOs.User;
using Comercio.Api.Service.Results;

namespace Comercio.Api.Service
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    }
}
