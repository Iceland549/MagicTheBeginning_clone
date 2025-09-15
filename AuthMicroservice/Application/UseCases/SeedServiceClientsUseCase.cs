using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using System.Threading.Tasks;

namespace AuthMicroservice.Application.UseCases
{
    public class SeedServiceClientsUseCase
    {
        private readonly IServiceClientRepository _repo;

        public SeedServiceClientsUseCase(IServiceClientRepository repo)
        {
            _repo = repo;
        }

        public async Task ExecuteAsync()
        {
            if (!await _repo.ExistsAsync("game-service"))
            {
                await _repo.AddAsync(new ServiceClient
                {
                    ClientId = "game-service",
                    ClientSecretHash = BCrypt.Net.BCrypt.HashPassword("SuperSecretGame"),
                    AllowedScopes = "deck.read"
                });
            }

            if (!await _repo.ExistsAsync("deck-service"))
            {
                await _repo.AddAsync(new ServiceClient
                {
                    ClientId = "deck-service",
                    ClientSecretHash = BCrypt.Net.BCrypt.HashPassword("SuperSecretDeck"),
                    AllowedScopes = "deck.read,deck.write"
                });
            }
        }
    }
}
