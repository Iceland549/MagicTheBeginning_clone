using Microsoft.EntityFrameworkCore;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using System.Threading.Tasks;
using System.Linq;

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
            // Utilise BCrypt pour hacher le mot de passe
            user.BCryptPasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.HashVersion = 2; // 2 = BCrypt
            user.PasswordHash = null; // Pas besoin pour BCrypt
            user.PasswordSalt = null; // Pas besoin pour BCrypt
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return false;

            bool isValid;
            if (user.HashVersion == 1) // Ancien format (HMACSHA512)
            {
                if (user.PasswordSalt == null || user.PasswordHash == null) return false; // Vérification null
                using var hmac = new HMACSHA512(user.PasswordSalt);
                var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                isValid = Enumerable.SequenceEqual(computed, user.PasswordHash);

                // Si valide, migre vers BCrypt
                if (isValid)
                {
                    user.BCryptPasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    user.HashVersion = 2;
                    user.PasswordHash = null; // Efface ancien hash
                    user.PasswordSalt = null; // Efface ancien salt
                    await _ctx.SaveChangesAsync();
                }
            }
            else // BCrypt (HashVersion == 2)
            {
                if (user.BCryptPasswordHash == null) return false; // Vérification null
                isValid = BCrypt.Net.BCrypt.Verify(password, user.BCryptPasswordHash);
            }

            return isValid;
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