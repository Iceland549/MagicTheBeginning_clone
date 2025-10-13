using CardMicroservice.Application.DTOs;

namespace CardMicroservice.Application.Interfaces
{
    /// <summary>
    /// Interface définissant les opérations de persistence pour les cartes (MongoDB).
    /// Elle agit comme un contrat entre la logique métier (UseCases) et la base de données.
    /// </summary>
    public interface ICardRepository
    {
        /// <summary>
        /// Ajoute une nouvelle carte dans la base Mongo.
        /// Si la carte vient de Scryfall, elle doit déjà être mappée en CardDto.
        /// </summary>
        Task AddAsync(CardDto card);

        /// <summary>
        /// Récupère toutes les cartes stockées.
        /// </summary>
        Task<List<CardDto>> GetAllAsync();

        /// <summary>
        /// Récupère une carte via son identifiant unique (ScryfallId ou ObjectId).
        /// </summary>
        Task<CardDto?> GetByIdAsync(string id);

        /// <summary>
        /// Recherche une carte via son nom et éventuellement son set, sa langue, ou son collectorNumber.
        /// ⚙️ Utilisée surtout quand on veut importer une carte depuis Scryfall.
        /// </summary>
        Task<CardDto?> GetByNameAsync(string name, string? set = null, string? lang = null, string? collectorNumber = null);

        /// <summary>
        /// Supprime une carte via son identifiant unique.
        /// </summary>
        Task<bool> DeleteByIdAsync(string id);
    }
}
