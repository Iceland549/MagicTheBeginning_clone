using Xunit;
using Moq;
using AutoMapper;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Application.UseCases;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameMicroservice.Tests
{
    public class PlayCardUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsError_WhenNotPlayersTurn()
        {
            // Arrange
            var session = new GameSession
            {
                Id = "game1",
                ActivePlayerId = "p1",
                CurrentPhase = Phase.Main
            };
            var action = new PlayerActionDto
            {
                PlayerId = "p2",
                Type = GameMicroservice.Application.DTOs.ActionType.PlayCard,
                CardId = "card1"
            };

            var mockRepo = new Mock<IGameSessionRepository>();
            mockRepo.Setup(r => r.GetByIdAsync("game1")).ReturnsAsync(session);

            var mockEngine = new Mock<IGameRulesEngine>();
            // Simule que le moteur rejette l'action car ce n'est pas le tour du joueur
            mockEngine
                .Setup(e => e.ValidatePlayAsync(session, "p2", "card1"))
                .ThrowsAsync(new InvalidOperationException("Pas la phase principale"));

            var mockMapper = new Mock<IMapper>();

            var useCase = new PlayCardUseCase(mockRepo.Object, mockEngine.Object, mockMapper.Object);

            // Act
            var result = await useCase.ExecuteAsync("game1", "p2", action);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Pas la phase principale", result.Message);
        }

        [Fact]
        public async Task ExecuteAsync_Succeeds_WhenValidPlay()
        {
            // Arrange
            var session = new GameSession
            {
                Id = "game1",
                ActivePlayerId = "p1",
                CurrentPhase = Phase.Main,
                Zones = new Dictionary<string, List<CardInGame>>
                {
                    { "p1_hand", new List<CardInGame> { new CardInGame("card1") } },
                    { "p1_battlefield", new List<CardInGame>() }
                },
                Players = new List<PlayerState>
                {
                    new PlayerState
                    {
                        PlayerId = "p1",
                        ManaPool = new Dictionary<string, int> { { "Colorless", 1 } }
                    }
                }
            };

            var action = new PlayerActionDto
            {
                PlayerId = "p1",
                Type = GameMicroservice.Application.DTOs.ActionType.PlayCard,
                CardId = "card1"
            };

            var mockRepo = new Mock<IGameSessionRepository>();
            mockRepo.Setup(r => r.GetByIdAsync("game1")).ReturnsAsync(session);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<GameSession>())).Returns(Task.CompletedTask);

            var mockEngine = new Mock<IGameRulesEngine>();
            mockEngine.Setup(e => e.IsMainPhase(session, "p1")).Returns(true);
            mockEngine.Setup(e => e.ValidatePlayAsync(session, "p1", "card1")).Returns(Task.CompletedTask);
            mockEngine.Setup(e => e.PlayCardAsync(session, "p1", "card1")).ReturnsAsync(session);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<ActionResultDto>(It.IsAny<GameSession>()))
                      .Returns(new ActionResultDto { Success = true, Message = "Carte jouée" });

            var useCase = new PlayCardUseCase(mockRepo.Object, mockEngine.Object, mockMapper.Object);

            // Act
            var result = await useCase.ExecuteAsync("game1", "p1", action);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Carte jouée", result.Message);
            mockRepo.Verify(r => r.UpdateAsync(session), Times.Once);
        }
    }
}
