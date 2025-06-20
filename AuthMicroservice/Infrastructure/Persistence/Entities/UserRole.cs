using System.ComponentModel.DataAnnotations.Schema;

namespace AuthMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Join entity for the many-to-many association between users and roles.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// ID of the associated user.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation to the associated User entity.
        /// Allows EF Core to automatically load related User data when needed,
        /// and simplifies querying (e.g. Include(u => u.UserRoles)).
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// ID of the associated role.
        /// </summary>
        public string RoleId { get; set; } = null!;

        /// <summary>
        /// Navigation to the associated Role entity.
        /// Enables direct access to Role properties (e.g. Role.Name) without extra queries.
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;
    }
}