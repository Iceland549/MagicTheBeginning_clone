using System.Collections.Generic;
using System.Threading.Tasks;
using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for accessing and managing role data in the persistence layer.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Retrieves a role by its unique identifier.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role.</param>
        /// <returns>The role if found, null otherwise.</returns>
        Task<Role?> GetByIdAsync(string roleId);

        /// <summary>
        /// Retrieves all roles available in the system.
        /// </summary>
        /// <returns>A list of all roles.</returns>
        Task<List<Role>> GetAllAsync();

        /// <summary>
        /// Creates a new role in the system.
        /// </summary>
        /// <param name="role">The role entity to create.</param>
        Task AddAsync(Role role);

        /// <summary>
        /// Updates an existing role with new information.
        /// </summary>
        /// <param name="role">The role entity with updated information.</param>
        Task UpdateAsync(Role role);

        /// <summary>
        /// Removes a role from the system by its identifier.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role to delete.</param>
        Task DeleteAsync(string roleId);
    }
}