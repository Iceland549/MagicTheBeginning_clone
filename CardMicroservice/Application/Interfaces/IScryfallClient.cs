using CardMicroservice.Infrastructure.Scryfall;

namespace CardMicroservice.Application.Interfaces
{
    // Récupère le DTO brut de Scryfall
    public interface IScryfallClient
    {
        Task<ScryfallCardDto?> FetchByNameAsync(string name, string? set = null, string? lang = null, string? collectorNumber = null);
        Task<ScryfallCardDto?> FetchByIdAsync(string scryfallId); 

    }
}