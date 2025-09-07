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
            var existing = new CardDto { Id = "1", Name = "Lightning Bolt" };
            mockRepo.Setup(r => r.GetByNameAsync("Lightning Bolt")).ReturnsAsync(existing);

            // ImportUseCase can be a real instance with mocked deps; it won't be called in this scenario
            var dummyClient = new Mock<IScryfallClient>();
            var dummyRepo = new Mock<ICardRepository>();
            var dummyMapper = new Mock<IMapper>();
            var importUseCase = new ImportCardUseCase(dummyClient.Object, dummyRepo.Object, dummyMapper.Object);

            var useCase = new GetCardByNameUseCase(mockRepo.Object, importUseCase);

            // Act
            var result = await useCase.ExecuteAsync("Lightning Bolt");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Lightning Bolt", result.Name);

            // Ensure we did not trigger remote fetch (import)
            dummyClient.Verify(c => c.FetchByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_Imports_WhenMissing()
        {
            // Arrange
            var mockRepo = new Mock<ICardRepository>();
            mockRepo.Setup(r => r.GetByNameAsync("NonExisting")).ReturnsAsync((CardDto?)null);

            // Real ImportUseCase but with mocked internals to control output
            var mockClient = new Mock<IScryfallClient>();
            var mockImportRepo = new Mock<ICardRepository>();
            var mockMapper = new Mock<IMapper>();

            var raw = new ScryfallCardDto { Name = "NewCard" };
            var importedDto = new CardDto { Id = "42", Name = "NewCard" };

            mockClient.Setup(c => c.FetchByNameAsync("NonExisting")).ReturnsAsync(raw);
            mockMapper.Setup(m => m.Map<CardDto>(raw)).Returns(importedDto);
            mockImportRepo.Setup(r => r.AddAsync(importedDto)).Returns(Task.CompletedTask);

            var importUseCase = new ImportCardUseCase(mockClient.Object, mockImportRepo.Object, mockMapper.Object);
            var useCase = new GetCardByNameUseCase(mockRepo.Object, importUseCase);

            // Act
            var result = await useCase.ExecuteAsync("NonExisting");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewCard", result.Name);
            mockImportRepo.Verify(r => r.AddAsync(It.Is<CardDto>(d => d.Name == "NewCard")), Times.Once);
        }
    }
}
