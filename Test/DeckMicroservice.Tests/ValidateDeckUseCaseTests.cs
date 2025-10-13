using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using DeckMicroservice.Application.UseCases;

namespace Test.DeckMicroservice.Tests
{
    public class ValidateDeckUseCaseTests
    {
        [Fact]
        public async Task ValidateDeck_ReturnsFalse_WhenDeckHasLessThan60Cards()
        {
            // Arrange
            var mockCardClient = new Mock<ICardClient>();

            var swampCard = new CardDto
            {
                Id = "swamp-id",
                Name = "Swamp",
                TypeLine = "Basic Land — Swamp",
                Cmc = 0
            };

            mockCardClient
                .Setup(c => c.GetCardByIdAsync("swamp-id"))
                .ReturnsAsync(swampCard);

            var validateUseCase = new ValidateDeckUseCase(mockCardClient.Object);

            var deck = new CreateDeckRequest
            {
                OwnerId = "user-id",
                Name = "Invalid Deck",
                Cards = new List<DeckCardDto>
                {
                    new DeckCardDto { CardId = "swamp-id", Quantity = 40 }
                }
            };

            // Act
            var (isValid, errorMessage) = await validateUseCase.ExecuteAsync(deck);

            // Assert
            Assert.False(isValid);
            Assert.Contains("au moins 60 cartes", errorMessage);
            mockCardClient.Verify(c => c.GetCardByIdAsync("swamp-id"), Times.Once);
        } 

        [Fact]
        public async Task ValidateDeck_ReturnsFalse_WhenLessThan20Lands()
        {
            // Arrange
            var mockCardClient = new Mock<ICardClient>();

            var swampCard = new CardDto
            {
                Id = "swamp-id",
                Name = "Swamp",
                TypeLine = "Basic Land — Swamp",
                Cmc = 0
            };

            var boltCard = new CardDto
            {
                Id = "bolt-id",
                Name = "Lightning Bolt",
                TypeLine = "Instant",
                ManaCost = "{R}",
                Cmc = 1
            };

            mockCardClient.Setup(c => c.GetCardByIdAsync("swamp-id")).ReturnsAsync(swampCard);
            mockCardClient.Setup(c => c.GetCardByIdAsync("bolt-id")).ReturnsAsync(boltCard);

            var validateUseCase = new ValidateDeckUseCase(mockCardClient.Object);

            var deck = new CreateDeckRequest
            {
                OwnerId = "user-id",
                Name = "Deck sans assez de terrains",
                Cards = new List<DeckCardDto>
                {
                    new DeckCardDto { CardId = "swamp-id", Quantity = 19 },  // Seulement 19 terrains
                    new DeckCardDto { CardId = "bolt-id", Quantity = 4 }      // 4 Lightning Bolt (OK, pas 41 !)
                }
            };

            // Act
            var (isValid, errorMessage) = await validateUseCase.ExecuteAsync(deck);

            // Assert
            Assert.False(isValid);
            // Le deck échoue d'abord sur le total de cartes (23 < 60), puis sur les terrains
            Assert.True(
                errorMessage.Contains("au moins 60 cartes") || errorMessage.Contains("au moins 20 terrains"),
                $"Message d'erreur inattendu : {errorMessage}"
            );
        }

        [Fact]
        public async Task ValidateDeck_ReturnsFalse_WhenNonLandCardExceeds4Copies()
        {
            // Arrange
            var mockCardClient = new Mock<ICardClient>();

            var swampCard = new CardDto
            {
                Id = "swamp-id",
                Name = "Swamp",
                TypeLine = "Basic Land — Swamp",
                Cmc = 0
            };

            var boltCard = new CardDto
            {
                Id = "bolt-id",
                Name = "Lightning Bolt",
                TypeLine = "Instant",
                ManaCost = "{R}",
                Cmc = 1
            };

            mockCardClient.Setup(c => c.GetCardByIdAsync("swamp-id")).ReturnsAsync(swampCard);
            mockCardClient.Setup(c => c.GetCardByIdAsync("bolt-id")).ReturnsAsync(boltCard);

            var validateUseCase = new ValidateDeckUseCase(mockCardClient.Object);

            var deck = new CreateDeckRequest
            {
                OwnerId = "user-id",
                Name = "Deck avec trop de copies",
                Cards = new List<DeckCardDto>
                {
                    new DeckCardDto { CardId = "swamp-id", Quantity = 55 },  // Terrains OK (pas de limite)
                    new DeckCardDto { CardId = "bolt-id", Quantity = 5 }     // Trop de copies (>4)
                }
            };

            // Act
            var (isValid, errorMessage) = await validateUseCase.ExecuteAsync(deck);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Lightning Bolt", errorMessage);
            Assert.Contains("4 exemplaires", errorMessage);
        }

        [Fact]
        public async Task ValidateDeck_ReturnsFalse_WhenCardNotFound()
        {
            // Arrange
            var mockCardClient = new Mock<ICardClient>();

            mockCardClient
                .Setup(c => c.GetCardByIdAsync("invalid-id"))
                .ReturnsAsync((CardDto?)null);

            var validateUseCase = new ValidateDeckUseCase(mockCardClient.Object);

            var deck = new CreateDeckRequest
            {
                OwnerId = "user-id",
                Name = "Deck avec carte invalide",
                Cards = new List<DeckCardDto>
                {
                    new DeckCardDto { CardId = "invalid-id", Quantity = 60 }
                }
            };

            // Act
            var (isValid, errorMessage) = await validateUseCase.ExecuteAsync(deck);

            // Assert
            Assert.False(isValid);
            Assert.Contains("invalid-id", errorMessage);
            Assert.Contains("n'existe pas", errorMessage);
        }

        [Fact]
        public async Task ValidateDeck_ReturnsTrue_WithExactly60CardsAnd20Lands()
        {
            // Arrange
            var mockCardClient = new Mock<ICardClient>();

            var forestCard = new CardDto
            {
                Id = "forest-id",
                Name = "Forest",
                TypeLine = "Basic Land — Forest",
                Cmc = 0
            };

            var bearCard = new CardDto
            {
                Id = "bear-id",
                Name = "Grizzly Bears",
                TypeLine = "Creature — Bear",
                ManaCost = "{1}{G}",
                Cmc = 2
            };

            mockCardClient.Setup(c => c.GetCardByIdAsync("forest-id")).ReturnsAsync(forestCard);
            mockCardClient.Setup(c => c.GetCardByIdAsync("bear-id")).ReturnsAsync(bearCard);

            var validateUseCase = new ValidateDeckUseCase(mockCardClient.Object);

            var deck = new CreateDeckRequest
            {
                OwnerId = "player-1",
                Name = "Green Deck",
                Cards = new List<DeckCardDto>
                {
                    new DeckCardDto { CardId = "forest-id", Quantity = 20 },  // Exactement 20 terrains
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 },
                    new DeckCardDto { CardId = "bear-id", Quantity = 4 }   // Total = 60
                }
            };

            // Act
            var (isValid, errorMessage) = await validateUseCase.ExecuteAsync(deck);

            // Assert
            Assert.True(isValid, $"Erreur inattendue : {errorMessage}");
            Assert.Empty(errorMessage);
        }
    }
}