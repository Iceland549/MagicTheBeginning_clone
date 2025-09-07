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
            var mockRepo = new Mock<IDeckRepository>();

            // Simule une validation échouée
            mockRepo
                .Setup(r => r.ValidateAsync(It.IsAny<CreateDeckRequest>()))
                .ReturnsAsync(false);

            var validateUseCase = new ValidateDeckUseCase(mockRepo.Object);

            var deck = new CreateDeckRequest
            {
                OwnerId = "user-id",
                Name = "Invalid Deck",
                Cards = new List<DeckCardDto>
                {
                    new DeckCardDto { CardName = "Swamp", Quantity = 40 }
                }
            };

            // Act
            var (isValid, errorMessage) = await validateUseCase.ExecuteAsync(deck);

            // Assert
            Assert.False(isValid);
            mockRepo.Verify(r => r.ValidateAsync(It.IsAny<CreateDeckRequest>()), Times.Once);
        }

        [Fact]
        public async Task ValidateDeck_ReturnsTrue_WhenDeckHas60Cards()
        {
            // Arrange
            var mockRepo = new Mock<IDeckRepository>();

            // Simule une validation réussie
            mockRepo
                .Setup(r => r.ValidateAsync(It.IsAny<CreateDeckRequest>()))
                .ReturnsAsync(true);

            var validateUseCase = new ValidateDeckUseCase(mockRepo.Object);

            var deck = new CreateDeckRequest
            {
                OwnerId = "user-id",
                Name = "Valid Deck",
                Cards = new List<DeckCardDto>
                {
                    new DeckCardDto { CardName = "Swamp", Quantity = 60 }
                }
            };

            // Act
            var (isValid, errorMessage) = await validateUseCase.ExecuteAsync(deck);

            // Assert
            Assert.True(isValid);
            mockRepo.Verify(r => r.ValidateAsync(It.IsAny<CreateDeckRequest>()), Times.Once);
        }
    }
}
