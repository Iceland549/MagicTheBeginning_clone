namespace AuthMicroservice.Application.DTOs
{
    /// <summary>
    /// DTO representing a user's public profile with roles.
    /// </summary>
    public class ProfileDto
    {
        /// <summary>
        /// Unique identifier of the user.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// List of roles assigned to the user.
        /// </summary>
        public IList<string> Roles { get; set; } = new List<string>();
    }
}