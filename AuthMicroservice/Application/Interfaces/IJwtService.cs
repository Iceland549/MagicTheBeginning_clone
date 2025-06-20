using System.Collections.Generic;
using System.Security.Claims;

namespace AuthMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for JWT token generation, validation, and management operations.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a new JWT access token for the specified user with their roles.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roles">List of roles assigned to the user.</param>
        /// <returns>A signed JWT token string.</returns>
        string GenerateToken(string userId, List<string> roles);

        /// <summary>
        /// Validates a JWT token and extracts the claims principal if valid.
        /// </summary>
        /// <param name="token">The JWT token string to validate.</param>
        /// <returns>ClaimsPrincipal containing user claims if token is valid.</returns>
        /// <exception cref="SecurityTokenException">Thrown when the token is invalid or expired.</exception>
        ClaimsPrincipal ValidateToken(string token);
    }
}