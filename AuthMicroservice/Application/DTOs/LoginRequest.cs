namespace AuthMicroservice.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for user login requests.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The email address used to log in.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// The user's plain-text password.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}