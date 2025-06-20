using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Infrastructure.Config;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthMicroservice.Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtService
    {
        private readonly IUserRepository _repo;
        private readonly JwtSettings _settings;
        private readonly Dictionary<string, string> _refreshStore = new();

        public JwtTokenGenerator(IUserRepository repo, IOptions<JwtSettings> opts)
        {
            _repo = repo;
            _settings = opts.Value;
        }

        public async Task<JwtResponse?> LoginAsync(string email, string password)
        {
            if (!await _repo.ValidateCredentialsAsync(email, password))
                return null;

            var user = await _repo.GetByEmailAsync(email);
            if (user == null)
                return null; 

            var roles = await _repo.GetRolesAsync(user.Id);
            var access = CreateToken(user.Id, roles);
            var refresh = Guid.NewGuid().ToString();
            _refreshStore[refresh] = user.Id;

            return new JwtResponse
            {
                AccessToken = access,
                RefreshToken = refresh,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes)
            };
        }


        public async Task<JwtResponse?> RefreshTokenAsync(string token)
        {
            if (!_refreshStore.TryGetValue(token, out var uid))
                return null;

            var roles = await _repo.GetRolesAsync(uid);
            var access = CreateToken(uid, roles);

            // Rotation du refresh token
            _refreshStore.Remove(token);
            var newRefresh = Guid.NewGuid().ToString();
            _refreshStore[newRefresh] = uid;

            return new JwtResponse
            {
                AccessToken = access,
                RefreshToken = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes)
            };
        }

        public Task LogoutAsync(string refreshToken)
        {
            _refreshStore.Remove(refreshToken);
            return Task.CompletedTask;
        }

        public string GenerateToken(string userId, List<string> roles)
        {
            return CreateToken(userId, roles);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret))
            };

            try
            {
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return principal;
            }
            catch
            {
                throw new SecurityTokenException("Invalid token"); ;
            }
        }



        private string CreateToken(string userId, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}