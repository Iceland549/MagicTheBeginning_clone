using System.Threading.Tasks;
using Xunit;
using Moq;
using CardMicroservice.Application.UseCases;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Application.DTOs;
using System.Collections.Generic;

namespace Test.CardMicroservice.Tests
{
    public class GetAllCardsUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsAllCards_WithCollector()
        {
            // Arrange
            var mockRepo = new Mock<ICardRepository>();

            // 🔹 Simule deux cartes avec CollectorNumber pour coller au nouveau modèle
            var list = new List<CardDto>
            {
                new CardDto
                {
                    Name = "Serra Angel",
                    CollectorNumber = "34",
                    Set = "3ed",
                    Lang = "en"
                },
                new CardDto
                {
                    Name = "White Knight",
                    CollectorNumber = "44",
                    Set = "3ed",
                    Lang = "en"
                }
            };

            // 🔹 Mock du repository
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            var useCase = new GetAllCardsUseCase(mockRepo.Object);

            // Act
            var result = await useCase.ExecuteAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Vérifie les noms et collector
            Assert.Contains(result, c => c.Name == "Serra Angel" && c.CollectorNumber == "34");
            Assert.Contains(result, c => c.Name == "White Knight" && c.CollectorNumber == "44");

            // Vérifie que le repo a bien été appelé
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }
    }
}
