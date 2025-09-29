//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace AuthMicroservice.Infrastructure.Persistence.Entities
//{
//    /// <summary>
//    /// Represents an email token used for confirmation or password reset.
//    /// </summary>
//    public class EmailToken
//    {
//        /// <summary>
//        /// Unique token identifier.
//        /// </summary>
//        [Key]
//        public string Id { get; set; } = Guid.NewGuid().ToString();

//        /// <summary>
//        /// The ID of the user the token is associated with.
//        /// </summary>
//        [Required]
//        public string UserId { get; set; } = null!;

//        [ForeignKey(nameof(UserId))]
//        public User User { get; set; } = null!;     // Navigation to User

//        /// <summary>
//        /// The token value used for verification.
//        /// </summary>
//        public string Token { get; set; } = null!;

//        /// <summary>
//        /// Expiration date of the token.
//        /// </summary>
//        public DateTime Expiration { get; set; }

//        /// <summary>
//        /// Type of token: email confirmation or password reset.
//        /// </summary>
//        public EmailTokenType Type { get; set; }

//        /// <summary>
//        /// Indicates whether the token has already been used.
//        /// </summary>
//        public bool Used { get; set; } = false;

//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//        public DateTime? UsedAt { get; set; }       // When it was used
//    }

//    /// <summary>
//    /// Enumeration of token types.
//    /// </summary>
//    public enum EmailTokenType
//    {
//        Confirmation,
//        ResetPassword
//    }
//}