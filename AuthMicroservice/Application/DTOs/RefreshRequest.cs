namespace AuthMicroservice.Application.DTOs
{
    /// <summary>
    /// DTO for requesting a new access token using a refresh token.
    /// </summary>
    public class RefreshRequest
    {
        /// <summary>
        /// The refresh token provided by the client.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
    }
}