using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Config;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Sends a reset password link to a user by generating a secure token.
    /// </summary>
    public class GenerateResetPasswordUseCase
    {
        private readonly AuthDbContext _context;

        public GenerateResetPasswordUseCase(
            AuthDbContext context
            )
        {
            _context = context;
        }

        public async Task ExecuteAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new KeyNotFoundException("User not found.");

            var token = Guid.NewGuid().ToString("N");

            await _context.SaveChangesAsync();

        }
    }
}