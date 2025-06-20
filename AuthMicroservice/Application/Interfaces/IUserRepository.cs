using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for accessing and managing user data in the persistence layer.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Finds a user by their email address.
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Creates a new user and hashes their password.
        /// </summary>
        Task CreateUserAsync(User user, string password);

        /// <summary>
        /// Verifies the email and password credentials of a user.
        /// </summary>
        Task<bool> ValidateCredentialsAsync(string email, string password);

        /// <summary>
        /// Assigns a new role to a user.
        /// </summary>
        Task AddRoleAsync(string userId, string role);

        /// <summary>
        /// Retrieves all roles associated with the user.
        /// </summary>
        Task<IList<string>> GetRolesAsync(string userId);

        /// <summary>
        /// Gets the user by their unique ID.
        /// </summary>
        Task<User?> GetByIdAsync(string userId);
    }
}