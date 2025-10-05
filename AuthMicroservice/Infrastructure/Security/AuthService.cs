using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Config;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;

namespace AuthMicroservice.Infrastructure.Security
{
    /// <summary>
    /// Implements IAuthService: orchestrates login, refresh, logout flows.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

    // Default refresh token life in days
    private const int RefreshTokenTtlDays = 30;

        public AuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshRepo,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtOptions)
        {
            _userRepo = userRepo;
            _refreshRepo = refreshRepo;
            _jwtService = jwtService;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<JwtResponse?> LoginAsync(string email, string password)
        {
            if (!await _userRepo.ValidateCredentialsAsync(email, password))
                return null;

            var user = await _userRepo.GetByEmailAsync(email)!
                         ?? throw new InvalidOperationException("User not found after validation");
            var roles = await _userRepo.GetRolesAsync(user.Id);

            // Generate access token
            var accessToken = _jwtService.GenerateToken(user.Id, new List<string>(roles));
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

            // Generate secure refresh token
            var refreshTokenValue = GenerateSecureToken();
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenTtlDays)
            };
            await _refreshRepo.CreateAsync(refreshToken);

            return new JwtResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = expiresAt,
                UserId = user.Id
            };
        }

        public async Task<JwtResponse?> RefreshTokenAsync(string refreshToken)
        {
            // Vérifier que le refresh token existe et est valide
            var existing = await _refreshRepo.GetByTokenAsync(refreshToken);
            if (existing == null || existing.ExpiresAt < DateTime.UtcNow)
                return null;

            // Récupérer l'utilisateur lié
            var user = await _userRepo.GetByIdAsync(existing.UserId);
            if (user == null)
                return null;

            // Récupérer les rôles de l’utilisateur
            var roles = await _userRepo.GetRolesAsync(user.Id);

            // Générer un nouveau JWT access token
            var newAccess = _jwtService.GenerateToken(user.Id, new List<string>(roles));
            var newExpires = DateTime.UtcNow.AddMinutes(60);

            // Générer un nouveau refresh token
            var newRefresh = Guid.NewGuid().ToString("N");
            await _refreshRepo.CreateAsync(new RefreshToken
            {
                Token = newRefresh,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });

            // Invalider l'ancien refresh token
            await _refreshRepo.InvalidateAsync(refreshToken);

            //  Retourner une réponse complète avec UserId
            return new JwtResponse
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh,
                ExpiresAt = newExpires,
                UserId = user.Id   
            };
        }


        public async Task LogoutAsync(string refreshToken)
        {
            await _refreshRepo.InvalidateAsync(refreshToken);
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}