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
        public async Task ExecuteAsync_ReturnsAllCards()
        {
            // Arrange
            var mockRepo = new Mock<ICardRepository>();
            var list = new List<CardDto> { new CardDto { Id = "1", Name = "A" }, new CardDto { Id = "2", Name = "B" } };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            var useCase = new GetAllCardsUseCase(mockRepo.Object);

            // Act
            var result = await useCase.ExecuteAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "A");
        }
    }
}
