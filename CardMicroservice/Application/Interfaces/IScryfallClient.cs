using CardMicroservice.Infrastructure.Scryfall;

namespace CardMicroservice.Application.Interfaces
{
    // Récupère le DTO brut de Scryfall
    public interface IScryfallClient
    {
        Task<ScryfallCardDto?> FetchByNameAsync(string name);
    }
}