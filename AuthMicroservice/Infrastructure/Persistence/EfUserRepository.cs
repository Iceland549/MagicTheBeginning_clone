using Microsoft.EntityFrameworkCore;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Infrastructure.Persistence
{
    public class EfUserRepository : IUserRepository
    {
        private readonly AuthDbContext _ctx;
        public EfUserRepository(AuthDbContext ctx) => _ctx = ctx;

        public async Task<User?> GetByEmailAsync(string email) =>
            await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task CreateUserAsync(User user, string password)
        {
            // Génère salt/hash
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            user.PasswordSalt = hmac.Key;
            user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return false;

            using var hmac = new System.Security.Cryptography.HMACSHA512(user.PasswordSalt);
            var computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(user.PasswordHash);
        }

        public async Task AddRoleAsync(string userId, string role)
        {
            _ctx.UserRoles.Add(new UserRole { UserId = userId, RoleId = role });
            await _ctx.SaveChangesAsync();
        }

        public async Task<IList<string>> GetRolesAsync(string userId) =>
            await _ctx.UserRoles
                      .Where(ur => ur.UserId == userId)
                      .Select(ur => ur.RoleId)
                      .ToListAsync();

        public async Task<User?> GetByIdAsync(string userId) =>
            await _ctx.Users.FindAsync(userId);
    }
}