using Comercio.Api.Models;

namespace Comercio.Api.Repository
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task CreateUserAsync(User user);
    }
}
