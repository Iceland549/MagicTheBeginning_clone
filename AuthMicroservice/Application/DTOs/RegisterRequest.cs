namespace AuthMicroservice.Application.DTOs
{
    /// <summary>
    /// DTO for registering a new user.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// The email address of the new user.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// The plain-text password chosen by the user.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}