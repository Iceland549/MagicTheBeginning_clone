using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using CardMicroservice.Application.UseCases;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Infrastructure.Scryfall;

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

            var raw = new ScryfallCardDto
            {
                Name = "Black Lotus",
                CollectorNumber = "233",
                Lang = "en"
            };

            var dto = new CardDto
            {
                Name = "Black Lotus",
                CollectorNumber = "233",
                Lang = "en"
            };

            // 🔹 Mock du client Scryfall (signature à 4 params)
            mockClient
                .Setup(c => c.FetchByNameAsync(
                    It.Is<string>(s => s == "Black Lotus"),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(raw);

            // 🔹 Mock du mapper et du repository
            mockMapper.Setup(m => m.Map<CardDto>(raw)).Returns(dto);
            mockRepo.Setup(r => r.AddAsync(dto)).Returns(Task.CompletedTask);

            var useCase = new ImportCardUseCase(mockClient.Object, mockRepo.Object, mockMapper.Object);

            // Act
            var result = await useCase.ExecuteAsync("Black Lotus", null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Black Lotus", result.Name);
            Assert.Equal("233", result.CollectorNumber);

            mockRepo.Verify(r => r.AddAsync(It.Is<CardDto>(x => x.Name == "Black Lotus")), Times.Once);
            mockClient.Verify(
                c => c.FetchByNameAsync(
                    It.Is<string>(s => s == "Black Lotus"),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()),
                Times.Once);
        }
    }
}
