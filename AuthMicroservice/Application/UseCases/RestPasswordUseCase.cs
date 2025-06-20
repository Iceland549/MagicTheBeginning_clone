using System.Security.Cryptography;
using System.Text;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    public class ResetPasswordUseCase
    {
        private readonly AuthDbContext _context;

        public ResetPasswordUseCase(AuthDbContext context) => _context = context;

        /// <summary>
        /// Replaces the user's password after validating the reset token.
        /// </summary>
        /// <param name="token">Reset password token.</param>
        /// <param name="newPassword">New password in plain text.</param>
        public async Task ExecuteAsync(string token, string newPassword)
        {
            var record = await _context.EmailTokens
                .FirstOrDefaultAsync(t => t.Token == token && t.Type == EmailTokenType.ResetPassword && !t.Used)
                ?? throw new InvalidOperationException("Invalid or used token.");

            if (record.Expiration < DateTime.UtcNow)
                throw new InvalidOperationException("Token has expired.");

            var user = await _context.Users.FindAsync(record.UserId)
                ?? throw new KeyNotFoundException("User not found.");

            using var hmac = new HMACSHA512();
            user.PasswordSalt = hmac.Key;
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(newPassword));

            record.Used = true;
            await _context.SaveChangesAsync();
        }
    }
}