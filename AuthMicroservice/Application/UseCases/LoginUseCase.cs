using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Application.DTOs;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Handles user login and delegates to the authentication service.
    /// </summary>
    public class LoginUseCase
    {
        private readonly IAuthService _auth;

        public LoginUseCase(IAuthService auth) => _auth = auth;

        /// <summary>
        /// Validates the credentials and returns a JWT token if valid.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="password">User password.</param>
        /// <returns>JWT token if login is successful.</returns>
        public Task<JwtResponse?> ExecuteAsync(string email, string password) =>
            _auth.LoginAsync(email, password);
    }
}