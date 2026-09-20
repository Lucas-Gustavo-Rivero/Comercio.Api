using Comercio.Api.Models;

namespace Comercio.Api.DTOs.User
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
