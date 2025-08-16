using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using GameMicroservice.Domain;
using Microsoft.Extensions.Options;
using GameMicroservice.Infrastructure.Config;

namespace GameMicroservice.Infrastructure.Persistence
{
    public class MongoGameSessionRepository : IGameSessionRepository
    {
        private readonly IMongoCollection<GameSession> _col;
        private readonly IMapper _mapper;
        private readonly IDeckClient _deckClient;
        private readonly ICardClient _cardClient;


        public MongoGameSessionRepository(IMongoDatabase db, IOptions<MongoDbConfig> mongoConfig, IMapper mapper, IDeckClient deckClient, ICardClient cardClient)
        {
            _col = db.GetCollection<GameSession>(mongoConfig.Value.GameCollection);
            _mapper = mapper;
            _deckClient = deckClient;
            _cardClient = cardClient;
        }

        public async Task<GameSession> CreateAsync(string playerOneId, string playerTwoId, string deckIdP1, string deckIdP2)
        {
            var allDecks = await _deckClient.GetAllDecksAsync();
            var p1Deck = allDecks.FirstOrDefault(d => d.Id == deckIdP1)
                         ?? throw new InvalidOperationException($"No deck found with ID {deckIdP1}");
            var p2Deck = allDecks.FirstOrDefault(d => d.Id == deckIdP2)
                         ?? throw new InvalidOperationException($"No deck found with ID {deckIdP2}");

            // Convert DeckDto.Cards to List<string> (cardName or cardId)
            var p1Library = Shuffle(p1Deck.Cards.SelectMany(c => Enumerable.Repeat(c.CardName, c.Quantity)).ToList());
            var p2Library = Shuffle(p2Deck.Cards.SelectMany(c => Enumerable.Repeat(c.CardName, c.Quantity)).ToList());

            var session = new GameSession
            {
                PlayerOneId = playerOneId,
                PlayerTwoId = playerTwoId,
                ActivePlayerId = playerOneId,
                CurrentPhase = Phase.Draw,
                Zones = new Dictionary<string, List<CardInGame>>
        {
            { $"{playerOneId}_library", new List<CardInGame>() },
            { $"{playerOneId}_hand", new List<CardInGame>() },
            { $"{playerOneId}_battlefield", new List<CardInGame>() },
            { $"{playerOneId}_graveyard", new List<CardInGame>() },
            { $"{playerTwoId}_library", new List<CardInGame>() },
            { $"{playerTwoId}_hand", new List<CardInGame>() },
            { $"{playerTwoId}_battlefield", new List<CardInGame>() },
            { $"{playerTwoId}_graveyard", new List<CardInGame>() }
        },
                Players = new List<PlayerState>
        {
            new PlayerState
            {
                PlayerId = playerOneId,
                LifeTotal = 20,
                ManaPool = new Dictionary<string, int>
                {
                    { "White", 0 }, { "Blue", 0 }, { "Black", 0 }, { "Red", 0 }, { "Green", 0 }
                },
                LandsPlayedThisTurn = 0,
                HasDrawnThisTurn = false
            },
            new PlayerState
            {
                PlayerId = playerTwoId,
                LifeTotal = 20,
                ManaPool = new Dictionary<string, int>
                {
                    { "White", 0 }, { "Blue", 0 }, { "Black", 0 }, { "Red", 0 }, { "Green", 0 }
                },
                LandsPlayedThisTurn = 0,
                HasDrawnThisTurn = false
            }
        }
            };

            // Affecte les bibliothèques
            session.Zones[$"{playerOneId}_library"] = p1Library.Select(name => new CardInGame(name)).ToList();
            session.Zones[$"{playerTwoId}_library"] = p2Library.Select(name => new CardInGame(name)).ToList();

            // Pioche initiale : 7 cartes pour chaque joueur
            for (int i = 0; i < 7; i++)
            {
                foreach (var playerId in new[] { playerOneId, playerTwoId })
                {
                    var library = session.Zones[$"{playerId}_library"];
                    if (library.Count > 0)
                    {
                        var card = library[0];
                        var details = await _cardClient.GetCardByIdAsync(card.CardId);
                        if (details != null)
                        {
                            session.Zones[$"{playerId}_hand"].Add(new CardInGame
                            {
                                CardId = details.Id,
                                Name = details.Name,
                                TypeLine = details.TypeLine,
                                ImageUrl = details.ImageUrl,
                                ManaCost = details.ManaCost,
                                Power = details.Power,
                                Toughness = details.Toughness,
                                IsTapped = false,
                                HasSummoningSickness = true
                            });
                        }
                        library.RemoveAt(0);
                    }
                }
            }


            await _col.InsertOneAsync(session);
            return session;
        }


        public async Task<GameSession?> GetByIdAsync(string gameId)
        {
            return await _col.Find(g => g.Id == gameId).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(GameSession session)
        {
            await _col.ReplaceOneAsync(g => g.Id == session.Id, session);
        }

        public async Task PlayCardAsync(string gameId, string cardName)
        {
            var session = await _col.Find(g => g.Id == gameId).FirstOrDefaultAsync()
                          ?? throw new InvalidOperationException("Partie introuvable");

            // Retire la carte de la main de l'active player
            var handKey = $"{session.ActivePlayerId}_hand";
            var cardInHand = session.Zones[handKey].FirstOrDefault(c => c.CardId == cardName)
                ?? throw new InvalidOperationException("Carte non présente en main");

            // Ajoute au champ de bataille
            session.Zones[handKey].Remove(cardInHand);
            var bfKey = $"{session.ActivePlayerId}_battlefield";
            session.Zones[bfKey].Add(cardInHand);

            // Passe au joueur suivant
            session.ActivePlayerId = session.ActivePlayerId == session.PlayerOneId
                ? session.PlayerTwoId
                : session.PlayerOneId;

            // Persist
            await _col.ReplaceOneAsync(g => g.Id == gameId, session);
        }
        private List<string> Shuffle(List<string> cards)
        {
            var rng = new Random();
            return cards.OrderBy(_ => rng.Next()).ToList();
        }
    }
}