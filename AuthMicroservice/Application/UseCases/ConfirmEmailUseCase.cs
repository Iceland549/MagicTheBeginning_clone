//using AuthMicroservice.Infrastructure.Persistence;
//using AuthMicroservice.Infrastructure.Persistence.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace AuthMicroservice.Application.UseCases
//{
//    /// <summary>
//    /// Confirms the user's email by validating a confirmation token.
//    /// </summary>
//    public class ConfirmEmailUseCase
//    {
//        private readonly AuthDbContext _context;

//        public ConfirmEmailUseCase(AuthDbContext context) => _context = context;

//        /// <summary>
//        /// Validates the email token and marks the user's email as confirmed.
//        /// </summary>
//        /// <param name="token">The confirmation token.</param>
//        public async Task ExecuteAsync(string token)
//        {
//            var record = await _context.EmailTokens
//                .FirstOrDefaultAsync(t => t.Token == token && t.Type == EmailTokenType.Confirmation && !t.Used)
//                ?? throw new InvalidOperationException("Invalid or used token.");

//            if (record.Expiration < DateTime.UtcNow)
//                throw new InvalidOperationException("Token has expired.");

//            var user = await _context.Users.FindAsync(record.UserId)
//                ?? throw new KeyNotFoundException("User not found.");

//            user.EmailConfirmed = true;
//            record.Used = true;

//            await _context.SaveChangesAsync();
//        }
//    }
//}