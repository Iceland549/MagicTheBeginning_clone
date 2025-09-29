using System.Security.Cryptography;
using System.Text;
using AuthMicroservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Resets the user's password for a given user ID.
    /// </summary>
    public class ResetPasswordUseCase
    {
        private readonly AuthDbContext _context;

        public ResetPasswordUseCase(AuthDbContext context) => _context = context;

        /// <summary>
        /// Replaces the user's password for the specified user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose password is to be reset.</param>
        /// <param name="newPassword">New password in plain text.</param>
        public async Task ExecuteAsync(string userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            using var hmac = new HMACSHA512();
            user.PasswordSalt = hmac.Key;
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(newPassword));

            await _context.SaveChangesAsync();
        }
    }
}