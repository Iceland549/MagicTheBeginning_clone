using DeckMicroservice.Application.DTOs;

namespace DeckMicroservice.Application.Interfaces
{
    public interface IDeckRepository
    {
        Task CreateAsync(CreateDeckRequest deck);
        Task<List<DeckDto>> GetByOwnerAsync(string ownerId);
        bool Validate(CreateDeckRequest deck);
    }
}