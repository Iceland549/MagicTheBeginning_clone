using System.Threading.Tasks;
using DeckMicroservice.Application.DTOs;

namespace DeckMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for interacting with the CardMicroservice.
    /// </summary>
    public interface ICardClient
    {
        /// <summary>
        /// Retrieves a card by its identifier from the CardMicroservice.
        /// </summary>
        /// <param name="cardId">The identifier of the card to retrieve.</param>
        /// <returns>The CardDto if found, or null if not found.</returns>
        Task<CardDto?> GetCardByIdAsync(string cardId);

        Task<List<CardDto>> GetAllAsync();

    }
}
