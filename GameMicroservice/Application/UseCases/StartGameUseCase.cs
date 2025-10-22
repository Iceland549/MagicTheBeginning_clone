using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Use case for starting a new game session between two players.
    /// </summary>
    public class StartGameUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IDeckClient _deckClient;
        private readonly ICardClient _cardClient;
        private readonly IMapper _mapper;

        public StartGameUseCase(
            IGameSessionRepository repo,
            IDeckClient deckClient,
            ICardClient cardClient,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _deckClient = deckClient ?? throw new ArgumentNullException(nameof(deckClient));
            _cardClient = cardClient ?? throw new ArgumentNullException(nameof(cardClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Executes the use case to start a game session.
        /// </summary>
        public async Task<GameSessionDto> ExecuteAsync(string p1, string p2, string deckIdP1, string deckIdP2)
        {
            // 1. Récupérer les decks
            var allDecks = await _deckClient.GetAllDecksAsync();
            var p1Deck = allDecks.FirstOrDefault(d => d.Id == deckIdP1)
                         ?? throw new InvalidOperationException($"No deck found with ID {deckIdP1}");
            var p2Deck = allDecks.FirstOrDefault(d => d.Id == deckIdP2)
                         ?? throw new InvalidOperationException($"No deck found with ID {deckIdP2}");

            // 2. Construire les bibliothèques mélangées
            var p1Library = Shuffle(p1Deck.Cards.SelectMany(c => Enumerable.Repeat(c.CardId, c.Quantity)).ToList());
            var p2Library = Shuffle(p2Deck.Cards.SelectMany(c => Enumerable.Repeat(c.CardId, c.Quantity)).ToList());

            // 3. Créer la session de jeu
            var session = new GameSession
            {
                PlayerOneId = p1,
                PlayerTwoId = p2,
                ActivePlayerId = p1,
                CurrentPhase = Phase.Draw,
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
                        ManaPool = new Dictionary<string, int>
                        {
                            { "White", 0 }, { "Blue", 0 }, { "Black", 0 }, { "Red", 0 }, { "Green", 0 }
                        },
                        LandsPlayedThisTurn = 0,
                        HasDrawnThisTurn = false
                    },
                    new PlayerState
                    {
                        PlayerId = p2,
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

            // 4. Affecter les bibliothèques
            session.Zones[$"{p1}_library"] = p1Library.Select(id => new CardInGame { CardId = id }).ToList();
            session.Zones[$"{p2}_library"] = p2Library.Select(id => new CardInGame { CardId = id }).ToList();

            // 5. Pioche initiale : 7 cartes pour chaque joueur
            for (int i = 0; i < 7; i++)
            {
                foreach (var playerId in new[] { p1, p2 })
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
                                HasSummoningSickness = false
                            });
                        }
                        library.RemoveAt(0);
                    }
                }
            }

            // 6. Persister via le repository
            await _repo.CreateAsync(session);

            // 7. Retourner le DTO
            return _mapper.Map<GameSessionDto>(session);
        }

        private List<string> Shuffle(List<string> cards)
        {
            var rng = new Random();
            return cards.OrderBy(_ => rng.Next()).ToList();
        }
    }
}