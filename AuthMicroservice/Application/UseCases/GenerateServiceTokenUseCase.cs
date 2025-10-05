using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthMicroservice.Application.UseCases
{
    public class GenerateServiceTokenUseCase
    {
        private readonly IServiceClientRepository _repo;
        private readonly IConfiguration _config;

        public GenerateServiceTokenUseCase(IServiceClientRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<ServiceTokenResponse> ExecuteAsync(ServiceTokenRequest req)
        {
            var client = await _repo.GetByClientIdAsync(req.ClientId)
                         ?? throw new UnauthorizedAccessException("Invalid client");

            if (!BCrypt.Net.BCrypt.Verify(req.ClientSecret, client.ClientSecretHash))
                throw new UnauthorizedAccessException("Invalid secret");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
              {
                  new Claim("client_id", client.ClientId),
                  new Claim(JwtRegisteredClaimNames.Sub, client.ClientId)

              };

            foreach (var scope in client.AllowedScopes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                claims.Add(new Claim("scope", scope.Trim()));

            var expires = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new ServiceTokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires
            };
        }
    }
}
