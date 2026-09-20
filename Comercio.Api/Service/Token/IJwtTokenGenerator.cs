using Comercio.Api.Models;

namespace Comercio.Api.Service.Token
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
