using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using CardMicroservice.Application.UseCases;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Infrastructure.Scryfall;
using System.Collections.Generic;

namespace Test.CardMicroservice.Tests
{
    public class ImportCardUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsCardAndSaves_WhenScryfallFound()
        {
            // Arrange
            var mockClient = new Mock<IScryfallClient>();
            var mockRepo = new Mock<ICardRepository>();
            var mockMapper = new Mock<IMapper>();

            var raw = new ScryfallCardDto { Name = "Black Lotus" }; // minimal non-null
            var dto = new CardDto { Id = "1", Name = "Black Lotus" };

            mockClient.Setup(c => c.FetchByNameAsync("Black Lotus")).ReturnsAsync(raw);
            mockMapper.Setup(m => m.Map<CardDto>(raw)).Returns(dto);
            mockRepo.Setup(r => r.AddAsync(dto)).Returns(Task.CompletedTask);

            var useCase = new ImportCardUseCase(mockClient.Object, mockRepo.Object, mockMapper.Object);

            // Act
            var result = await useCase.ExecuteAsync("Black Lotus");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Black Lotus", result.Name);
            mockRepo.Verify(r => r.AddAsync(It.Is<CardDto>(x => x.Name == "Black Lotus")), Times.Once);
        }
    }
}
