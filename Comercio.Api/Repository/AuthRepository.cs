using Comercio.Api.Data;
using Comercio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Comercio.Api.Repository
{
    public class AuthRepository:IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<bool> UserExistsAsync(string email)
        {
            return _context.Users.AnyAsync(u => u.Email == email);
        }
        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

    }
}
