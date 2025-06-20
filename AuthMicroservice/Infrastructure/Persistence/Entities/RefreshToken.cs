using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents a stored refresh token for JWT.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// The token string (primary key).
        /// </summary>
        [Key]
        public string Token { get; set; } = null!;  // Primary key

        /// <summary>
        /// The ID of the user this token belongs to.
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;     // Navigation to User

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Expiration date of this refresh token.
        /// </summary>
        public DateTime ExpiresAt { get; set; }           // Expiration date

        /// <summary>
        /// Whether this token has been revoked.
        /// </summary>
        public bool IsRevoked { get; set; } = false;      // Revocation flag

        public DateTime? RevokedAt { get; set; }          // When revoked

    }
}