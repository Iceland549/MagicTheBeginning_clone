using DeckMicroservice.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeckMicroservice.Application.Interfaces
{
    public interface IDeckRepository
    {
        /// <summary>
        /// Creates a new deck in the repository.
        /// </summary>
        Task AddAsync(DeckDto deck);

        /// <summary>
        /// Retrieves all decks belonging to a specific owner.
        /// </summary>
        Task<List<DeckDto>> GetByOwnerAsync(string ownerId);

        /// <summary>
        /// Retrieves all decks from the repository.
        /// </summary>
        Task<List<DeckDto>> GetAllDecksAsync();

        /// <summary>
        /// Retrieves a single deck by its unique ID.
        /// </summary>
        Task<DeckDto?> GetByIdAsync(string id);

        /// <summary>
        /// Checks if a specific card (by CardId) exists in any deck.
        /// </summary>
        Task<bool> ExistsCardAsync(string cardId);
    }
}