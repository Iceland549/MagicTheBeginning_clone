namespace AuthMicroservice.Application.DTOs
{
    /// <summary>
    /// DTO for representing an email-related token (confirmation or password reset).
    /// </summary>
    public class EmailTokenDto
    {
        /// <summary>
        /// The raw token value.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Expiration time of the token.
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// The type of token ("Confirmation" or "ResetPassword").
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Indicates whether the token has already been used.
        /// </summary>
        public bool Used { get; set; }
    }
}