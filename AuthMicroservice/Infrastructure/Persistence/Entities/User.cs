using System;
using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents a user of the application.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier (GUID) for the user.
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Email address of the user (must be a valid format).
        /// </summary>
        [EmailAddress]
        [Required]
        public string Email { get; set; } = null!;

        /// <summary>
        [Required]
        /// Hashed password stored securely.
        /// </summary>
        public byte[] PasswordHash { get; set; } = null!;

        /// <summary>
        [Required]
        /// Salt used when hashing the password.
        /// </summary>
        public byte[] PasswordSalt { get; set; } = null!;

        /// <summary>
        /// Whether the email address has been confirmed.
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Audit

        public DateTime? UpdatedAt { get; set; }                    // Audit

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<EmailToken> EmailTokens { get; set; } = new List<EmailToken>();

    }
}
