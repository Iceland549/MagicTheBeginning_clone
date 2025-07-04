namespace AuthMicroservice.Application.DTOs
{
    /// <summary>
    /// DTO returned after successful authentication containing access and refresh tokens.
    /// </summary>
    public class JwtResponse
    {
        /// <summary>
        /// The JWT access token for authenticated requests.
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// A refresh token to obtain a new access token after expiration.
        /// </summary>
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// Expiration time of the access token in UTC.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        public string UserId { get; set; } = null!;  
    }
}