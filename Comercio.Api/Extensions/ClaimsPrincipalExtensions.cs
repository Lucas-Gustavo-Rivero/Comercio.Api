using System.Security.Claims;

namespace Comercio.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if(string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("El usuario no esta autenticado o el token es invalido");
            }

            return int.Parse(userId);
        }
    }
}
