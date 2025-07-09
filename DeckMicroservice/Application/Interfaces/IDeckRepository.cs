using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Infrastructure.Persistence.Entities;

namespace DeckMicroservice.Application.Interfaces
{
    public interface IDeckRepository
    {
        Task CreateAsync(CreateDeckRequest deck);
        Task<List<DeckDto>> GetByOwnerAsync(string ownerId);
        Task<bool> ValidateAsync(CreateDeckRequest deck);
        Task<List<Deck>> GetAllDecksAsync();

    }
}