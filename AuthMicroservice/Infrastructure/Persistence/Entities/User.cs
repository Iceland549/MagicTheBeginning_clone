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
        /// Hashed password for HMACSHA512 (used when HashVersion == 1).
        /// </summary>
        public byte[]? PasswordHash { get; set; } // Nullable pour BCrypt

        /// <summary>
        /// Salt used for HMACSHA512 (used when HashVersion == 1).
        /// </summary>
        public byte[]? PasswordSalt { get; set; } // Nullable pour BCrypt

        /// <summary>
        /// Hashed password for BCrypt (used when HashVersion == 2).
        /// </summary>
        public string? BCryptPasswordHash { get; set; } // Pour BCrypt

        /// <summary>
        /// Whether the email address has been confirmed.
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Tracks hashing algorithm (1 = HMACSHA512, 2 = BCrypt).
        /// </summary>
        public int HashVersion { get; set; } = 1; // 1 = HMACSHA512 (default pour existing)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Audit

        public DateTime? UpdatedAt { get; set; }                    // Audit

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<EmailToken> EmailTokens { get; set; } = new List<EmailToken>();
    }
}