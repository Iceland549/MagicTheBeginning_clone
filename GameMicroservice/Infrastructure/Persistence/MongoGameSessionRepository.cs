using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using GameMicroservice.Domain;

namespace GameMicroservice.Infrastructure.Persistence
{
    public class MongoGameSessionRepository : IGameSessionRepository
    {
        private readonly IMongoCollection<GameSession> _col;
        private readonly IMapper _mapper;
        private readonly IDeckClient _deckClient;
        private readonly ICardClient _cardClient;


        public MongoGameSessionRepository(IConfiguration cfg, IMapper mapper, IDeckClient deckClient, ICardClient cardClient)
        {
            var client = new MongoClient(cfg["Mongo:ConnectionString"]);
            var db = client.GetDatabase(cfg["Mongo:Database"]);
            _col = db.GetCollection<GameSession>(cfg["Mongo:GameCollection"]);
            _mapper = mapper;
            _deckClient = deckClient;
            _cardClient = cardClient;
        }

        public async Task<GameSession> CreateAsync(string p1, string p2)
        {

            // Récupérer les decks via l'API Gateway
            var p1Decks = await _deckClient.GetDecksByOwnerAsync(p1);
            var p2Decks = await _deckClient.GetDecksByOwnerAsync(p2);

            // Sélectionner le premier deck valide pour chaque joueur (simplification)
            var p1Deck = p1Decks.FirstOrDefault() ?? throw new InvalidOperationException($"No deck found for player {p1}");
            var p2Deck = p2Decks.FirstOrDefault() ?? throw new InvalidOperationException($"No deck found for player {p2}");

            // Convertir DeckDto.Cards en List<string> (cardName ou cardId)
            var p1Library = p1Deck.Cards.SelectMany(c => Enumerable.Repeat(c.CardName, c.Quantity)).ToList();
            var p2Library = p2Deck.Cards.SelectMany(c => Enumerable.Repeat(c.CardName, c.Quantity)).ToList();

            // Mélanger les bibliothèques
            p1Library = Shuffle(p1Library);
            p2Library = Shuffle(p2Library);

            var session = new GameSession
            {
                PlayerOneId = p1,
                PlayerTwoId = p2,
                ActivePlayerId = p1,
                CurrentPhase = Phase.Draw, // Initialize game phase
                Zones = new Dictionary<string, List<CardInGame>>
                {
                    { $"{p1}_library", new List<CardInGame>() },
                    { $"{p1}_hand", new List<CardInGame>() },
                    { $"{p1}_battlefield", new List<CardInGame>() },
                    { $"{p1}_graveyard", new List<CardInGame>() },
                    { $"{p2}_library", new List<CardInGame>() },
                    { $"{p2}_hand", new List<CardInGame>() },
                    { $"{p2}_battlefield", new List<CardInGame>() },
                    { $"{p2}_graveyard", new List<CardInGame>() }
                },
                Players = new List<PlayerState>
                {
                    new PlayerState
                    {
                        PlayerId = p1,
                        LifeTotal = 20,
                        ManaPool = new Dictionary<GameMicroservice.Domain.Color, int>
                        {
                            { GameMicroservice.Domain.Color.White, 0 },
                            { GameMicroservice.Domain.Color.Blue, 0 },
                            { GameMicroservice.Domain.Color.Black, 0 },
                            { GameMicroservice.Domain.Color.Red, 0 },
                            { GameMicroservice.Domain.Color.Green, 0 }
                        },
                        LandsPlayedThisTurn = 0,
                        HasDrawnThisTurn = false
                    },
                    new PlayerState
                    {
                        PlayerId = p2,
                        LifeTotal = 20,
                        ManaPool = new Dictionary<GameMicroservice.Domain.Color, int>
                        {
                            { GameMicroservice.Domain.Color.White, 0 },
                            { GameMicroservice.Domain.Color.Blue, 0 },
                            { GameMicroservice.Domain.Color.Black, 0 },
                            { GameMicroservice.Domain.Color.Red, 0 },
                            { GameMicroservice.Domain.Color.Green, 0 }
                        },
                        LandsPlayedThisTurn = 0,
                        HasDrawnThisTurn = false
                    }
                }
            };
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
            session.Zones[bfKey].Add(new CardInGame(cardName));

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