using Xunit;
using Moq;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Application.UseCases;
using GameMicroservice.Infrastructure.Persistence.Entities;
using GameMicroservice.Infrastructure;
using System.Collections.Generic;

namespace GameMicroservice.Tests
{
    public class GameRuleEngineTests
    {
        [Fact]
        public void CheckEndGame_ReturnsNull_WhenNoWinner()
        {
            // Arrange
            var session = new GameSession
            {
                Players = new List<PlayerState>
        {
            new PlayerState { PlayerId = "p1", LifeTotal = 20 },
            new PlayerState { PlayerId = "p2", LifeTotal = 20 }
        },
                Zones = new Dictionary<string, List<CardInGame>>
        {
            { "p1_library", new List<CardInGame> { new CardInGame("c1") } },
            { "p2_library", new List<CardInGame> { new CardInGame("c2") } }
        }
            };

            var mockCardClient = new Moq.Mock<ICardClient>();
            var mockRepo = new Moq.Mock<IGameSessionRepository>();
            var engine = new GameRulesEngine(mockCardClient.Object, mockRepo.Object);

            // Act
            var endGame = engine.CheckEndGame(session);

            // Assert
            Assert.Null(endGame); 
        }


        [Fact]
        public void CheckEndGame_ReturnsWinner_WhenPlayerLifeZero()
        {
            // Arrange
            var session = new GameSession
            {
                Players = new List<PlayerState>
                {
                    new PlayerState { PlayerId = "p1", LifeTotal = 0 },
                    new PlayerState { PlayerId = "p2", LifeTotal = 20 }
                }
            };

            var mockCardClient = new Mock<ICardClient>();
            var mockRepo = new Mock<IGameSessionRepository>();

            var engine = new GameRulesEngine(mockCardClient.Object, mockRepo.Object);

            // Act
            var endGame = engine.CheckEndGame(session);

            // Assert
            Assert.NotNull(endGame);
            Assert.Equal("p2", endGame.WinnerId); // p2 gagne si p1 est à 0 PV
        }
    }
}
