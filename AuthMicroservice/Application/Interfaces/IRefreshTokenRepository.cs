using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for managing refresh tokens in persistence.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Stores a new refresh token.
        /// </summary>
        Task CreateAsync(RefreshToken refreshToken);

        /// <summary>
        /// Retrieves a refresh token by its token string.
        /// </summary>
        Task<RefreshToken?> GetByTokenAsync(string token);

        /// <summary>
        /// Marks a refresh token as revoked.
        /// </summary>
        Task InvalidateAsync(string token);
    }
}