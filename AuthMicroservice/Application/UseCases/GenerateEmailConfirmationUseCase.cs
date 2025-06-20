using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using AuthMicroservice.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Generates a confirmation email token and sends a confirmation link to the user.
    /// </summary>
    public class GenerateEmailConfirmationUseCase
    {
        private readonly AuthDbContext _context;
        private readonly IEmailService _emailService;
        private readonly SmtpSettings _smtp;

        public GenerateEmailConfirmationUseCase(
            AuthDbContext context,
            IEmailService emailService,
            IOptions<SmtpSettings> smtpOptions)
        {
            _context = context;
            _emailService = emailService;
            _smtp = smtpOptions.Value;
        }

        /// <summary>
        /// Generates and stores a confirmation token, then sends a confirmation email to the user.
        /// </summary>
        /// <param name="userEmail">User email address to confirm.</param>
        public async Task ExecuteAsync(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail)
                ?? throw new KeyNotFoundException("User not found.");

            var token = Guid.NewGuid().ToString("N");

            _context.EmailTokens.Add(new EmailToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                Type = EmailTokenType.Confirmation
            });

            await _context.SaveChangesAsync();

            var link = $"{_smtp.FrontendUrl}/confirm-email?token={token}";
            var body = $"<p>Hello,<br/>Please <a href='{link}'>click here</a> to confirm your email.</p>";
            await _emailService.SendEmailAsync(user.Email, "Confirm your account", body);
        }
    }
}