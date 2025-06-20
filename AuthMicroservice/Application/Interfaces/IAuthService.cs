using AuthMicroservice.Application.DTOs;

namespace AuthMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for authentication logic (login, refresh, logout).
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Validates the user's credentials and issues tokens.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's plain-text password.</param>
        /// <returns>A JWT response if successful, null otherwise.</returns>
        Task<JwtResponse?> LoginAsync(string email, string password);

        /// <summary>
        /// Generates a new JWT access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token issued previously.</param>
        /// <returns>A new JWT response if the refresh token is valid.</returns>
        Task<JwtResponse?> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Invalidates a refresh token (e.g., on logout).
        /// </summary>
        /// <param name="refreshToken">The token to invalidate.</param>
        Task LogoutAsync(string refreshToken);
    }
}