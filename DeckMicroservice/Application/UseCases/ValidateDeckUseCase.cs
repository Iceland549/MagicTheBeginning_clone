using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DeckMicroservice.Application.UseCases
{
    public class ValidateDeckUseCase
    {
        private readonly ICardClient _cardClient;

        public ValidateDeckUseCase(ICardClient cardClient)
        {
            _cardClient = cardClient;
        }

        /// <summary>
        /// Valide la structure et le contenu d’un deck avant création.
        /// Retourne (true, "") si tout est bon, sinon (false, message d’erreur).
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ExecuteAsync(CreateDeckRequest req)
        {
            if (req == null)
                return (false, "Le deck est nul.");

            if (string.IsNullOrWhiteSpace(req.OwnerId))
                return (false, "L’OwnerId est requis.");

            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Le nom du deck est requis.");

            if (req.Cards == null || !req.Cards.Any())
                return (false, "Le deck doit contenir au moins une carte.");

            int totalCards = req.Cards.Sum(c => c.Quantity);
            int landCount = 0;

            foreach (var cardEntry in req.Cards)
            {
                if (cardEntry.Quantity <= 0)
                    return (false, $"Quantité invalide pour la carte {cardEntry.CardId}.");

                var card = await _cardClient.GetCardByIdAsync(cardEntry.CardId);
                if (card == null)
                    return (false, $"La carte {cardEntry.CardId} n'existe pas.");

                bool isLand = card.TypeLine?.ToLowerInvariant().Contains("land") ?? false;
                if (!isLand && cardEntry.Quantity > 4)
                    return (false, $"La carte {card.Name} dépasse la limite de 4 exemplaires (hors terrains).");

                if (isLand)
                    landCount += cardEntry.Quantity;
            }

            if (totalCards < 60)
                return (false, "Le deck doit contenir au moins 60 cartes.");

            if (landCount < 20)
                return (false, "Le deck doit contenir au moins 20 terrains.");

            return (true, string.Empty);
        }
    }
}
