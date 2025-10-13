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
    public class GetCardByNameUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsExisting_WhenFound()
        {
            // Arrange
            var mockRepo = new Mock<ICardRepository>();
            var existing = new CardDto { Name = "Lightning Bolt" };

            // Nouvelle signature : (name, set, lang, collectorNumber)
            mockRepo
                .Setup(r => r.GetByNameAsync(
                    It.Is<string>(s => s == "Lightning Bolt"),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(existing);

            // Import use case (non utilisé ici)
            var dummyClient = new Mock<IScryfallClient>();
            var dummyRepo = new Mock<ICardRepository>();
            var dummyMapper = new Mock<IMapper>();
            var importUseCase = new ImportCardUseCase(dummyClient.Object, dummyRepo.Object, dummyMapper.Object);

            var useCase = new GetCardByNameUseCase(mockRepo.Object, importUseCase);

            // Act
            var result = await useCase.ExecuteAsync("Lightning Bolt", null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Lightning Bolt", result.Name);

            // Vérifie qu'on n'a pas tenté d'importer depuis Scryfall
            dummyClient.Verify(
                c => c.FetchByNameAsync(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_Imports_WhenMissing()
        {
            // Arrange
            var mockRepo = new Mock<ICardRepository>();
            mockRepo
                .Setup(r => r.GetByNameAsync(
                    It.Is<string>(s => s == "NonExisting"),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .ReturnsAsync((CardDto?)null);

            var mockClient = new Mock<IScryfallClient>();
            var mockImportRepo = new Mock<ICardRepository>();
            var mockMapper = new Mock<IMapper>();

            var raw = new ScryfallCardDto { Name = "NewCard" };
            var importedDto = new CardDto { Name = "NewCard" };

            // Signature complète : name, set, lang, collectorNumber
            mockClient
                .Setup(c => c.FetchByNameAsync(
                    It.Is<string>(s => s == "NonExisting"),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(raw);

            mockMapper
                .Setup(m => m.Map<CardDto>(raw))
                .Returns(importedDto);

            mockImportRepo
                .Setup(r => r.AddAsync(It.Is<CardDto>(d => d.Name == "NewCard")))
                .Returns(Task.CompletedTask);

            var importUseCase = new ImportCardUseCase(mockClient.Object, mockImportRepo.Object, mockMapper.Object);
            var useCase = new GetCardByNameUseCase(mockRepo.Object, importUseCase);

            // Act
            var result = await useCase.ExecuteAsync("NonExisting", null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewCard", result.Name);

            mockImportRepo.Verify(r => r.AddAsync(It.Is<CardDto>(d => d.Name == "NewCard")), Times.Once);
            mockClient.Verify(
                c => c.FetchByNameAsync(
                    It.Is<string>(s => s == "NonExisting"),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()),
                Times.Once);
        }
    }
}
