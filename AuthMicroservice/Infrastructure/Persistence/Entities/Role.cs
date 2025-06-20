using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents a user role (e.g., Admin, Player).
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Unique identifier of the role.
        /// </summary>
        [Key]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Name of the role.
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        // Navigation property for many-to-many relation
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
