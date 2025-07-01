using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.Interfaces;

namespace AuthMicroservice.Application.UseCases
{
    public class LogoutUseCase
    {
        private readonly IAuthService _auth;
        public LogoutUseCase(IAuthService authService)
        {
            _auth = authService;
        }
        /// <summary>
        /// Invalidates the provided refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to invalidate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));
            await _auth.LogoutAsync(refreshToken);
        }
        //    public LogoutUseCase(IAuthService auth) => _auth = auth;

        //    public Task ExecuteAsync(string refreshToken) => _auth.LogoutAsync(refreshToken);
        //}
    }
}
