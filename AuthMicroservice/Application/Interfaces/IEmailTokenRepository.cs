using System.Threading.Tasks;
using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for accessing and managing email token data in the persistence layer.
    /// </summary>
    public interface IEmailTokenRepository
    {
        /// <summary>
        /// Retrieves an email token by its token value.
        /// </summary>
        /// <param name="token">The token string to search for.</param>
        /// <returns>The email token if found, null otherwise.</returns>
        Task<EmailToken?> GetByTokenAsync(string token);

        /// <summary>
        /// Creates a new email token in the system.
        /// </summary>
        /// <param name="emailToken">The email token entity to create.</param>
        Task AddAsync(EmailToken emailToken);

        /// <summary>
        /// Updates an existing email token with new information.
        /// </summary>
        /// <param name="emailToken">The email token entity with updated information.</param>
        Task UpdateAsync(EmailToken emailToken);

        /// <summary>
        /// Removes an email token from the system by its token value.
        /// </summary>
        /// <param name="token">The token string to delete.</param>
        Task DeleteAsync(string token);
    }
}