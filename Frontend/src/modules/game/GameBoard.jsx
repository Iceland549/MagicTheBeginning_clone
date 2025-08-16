import React, { useState, useEffect } from 'react';
import { startGame, getGameState, playCard } from './gameService';
import { getAllDecks } from '../decks/deckService';
import { getAllCards } from '../cards/cardService'; // Ajout
import api from '../../services/api';
import { useNavigate } from 'react-router-dom';
import './GameBoard.css';
import PlayerZone from './PlayerZone';
import GameActions from './GameActions';
import Library from './Library';

export default function GameBoard() {
  const [gameId, setGameId] = useState('');
  const [state, setState] = useState(null);
  const [decks, setDecks] = useState([]);
  const [deckP1, setDeckP1] = useState(null);
  const [deckP2, setDeckP2] = useState(null);
  const [loading, setLoading] = useState(false);
  const [cardDetails, setCardDetails] = useState({}); // Nouvel état pour les détails des cartes
  const navigate = useNavigate();
  const playerId = localStorage.getItem('userId');

  useEffect(() => {
    if (!playerId) {
      navigate('/login');
      return;
    }
    // Récupérer les decks
    getAllDecks()
      .then(response => setDecks(response.data || []))
      .catch(() => setDecks([]));

    // Récupérer toutes les cartes pour les détails
    getAllCards()
      .then(response => {
        console.log('Cards fetched in GameBoard:', JSON.stringify(response, null, 2));
        const cardMap = response.reduce((map, card) => {
          map[card.id] = card; // Mapper par ID pour un accès rapide
          return map;
        }, {});
        setCardDetails(cardMap);
      })
      .catch(error => {
        console.error('Erreur lors de getAllCards:', error);
        setCardDetails({});
      });
  }, [playerId, navigate]);

  const refresh = async () => {
    if (!gameId) return;
    setLoading(true);
    try {
      const response = await getGameState(gameId);
      console.log('Game state:', JSON.stringify(response, null, 2));
      setState(response);
    } catch (error) {
      alert('Erreur lors du rafraîchissement de l’état du jeu');
    }
    setLoading(false);
  };

  const init = async () => {
    if (!deckP1 || !deckP2) {
      alert('Sélectionne un deck pour chaque joueur');
      return;
    }
    setLoading(true);
    try {
      const gameData = {
        playerOneId: playerId,
        playerTwoId: 'AI',
        deckIdP1: deckP1.id,
        deckIdP2: deckP2.id,
      };
      const response = await startGame(gameData);
      setGameId(response.id);
      setState(response);
    } catch (error) {
      alert(`Erreur lors du démarrage de la partie : ${error.response?.data?.error || error.message}`);
    }
    setLoading(false);
  };

  const onPlay = async (cardId, actionType = 'PlayCard') => {
    if (!gameId || !playerId) return;
    setLoading(true);
    try {
      await playCard(gameId, { playerId, cardId, type: actionType });
      await refresh();
    } catch (error) {
      alert('Erreur lors de l’action : ' + (error.message || ''));
    }
    setLoading(false);
  };

  useEffect(() => {
    if (state && state.activePlayerId === state.playerTwoId) {
      setLoading(true);
      api.post(`/games/${gameId}/ai-turn`)
        .then(() => refresh())
        .catch(() => setLoading(false));
    }
  }, [state?.activePlayerId, gameId]);

  const availableDecks = decks.filter(
    d => (!deckP1 || d.id !== deckP1.id) && (!deckP2 || d.id !== deckP2.id)
  );

  const buildActions = () => {
    if (!state || state.activePlayerId !== playerId) return [];

    const hand = state.zones[`${playerId}_hand`] || [];
    const landCards = hand.filter(c => c.typeLine?.includes('Land'));
    const spellCards = hand.filter(c => !c.typeLine?.includes('Land'));

    const actions = [];

    landCards.forEach(card => {
      actions.push({
        label: `Jouer Terrain: ${card.cardId}`,
        type: 'PlayLand',
        cardId: card.cardId
      });
    });

    spellCards.forEach(card => {
      actions.push({
        label: `Jouer Sort: ${card.cardId}`,
        type: 'PlayCard',
        cardId: card.cardId
      });
    });

    switch (state.currentPhase) {
      case 'Main':
        actions.push({ label: 'Passer à la phase de combat', type: 'PassToCombat' });
        actions.push({ label: 'Finir le tour', type: 'EndTurn' });
        break;
      case 'Combat':
        actions.push({ label: 'Fin de combat', type: 'PreEnd' });
        break;
      case 'End':
        actions.push({ label: 'Finir le tour', type: 'EndTurn' });
        break;
      default:
        break;
    }

    return actions;
  };

  // Enrichir les zones avec les détails des cartes
  const enrichedZones = state
    ? Object.keys(state.zones).reduce((acc, zoneKey) => {
        acc[zoneKey] = state.zones[zoneKey].map(card => ({
          ...card,
          imageUrl: cardDetails[card.cardId]?.imageUrl || card.imageUrl || 'https://via.placeholder.com/100'
        }));
        return acc;
      }, {})
    : {};

  const handleAction = async (action) => {
    await onPlay(action.cardId || null, action.type);
  };

  return (
    <div
      className="game-table-bg"
      style={{
        backgroundImage: "url('/blood-artist.jpg'), linear-gradient(135deg,#222 0%,#444 100%)"
      }}
    >
      <div className="game-table-container">
        <button className="btn-back" onClick={() => navigate('/dashboard')}>
          ← Retour au Dashboard
        </button>
        <h2>Plateau de jeu</h2>

        {!state && (
          <>
            <div className="available-decks-zone">
              <h3>Decks disponibles</h3>
              <ul className="deck-list">
                {availableDecks.map(deck => (
                  <li key={deck.id} className="deck-item">
                    <span>
                      {deck.name} <span className="deck-count">{deck.cards.reduce((sum, c) => sum + c.quantity, 0)} cartes</span>
                    </span>
                    <span>
                      <button className="btn-assign" onClick={() => setDeckP1(deck)}>+ Joueur 1</button>
                      <button className="btn-assign" onClick={() => setDeckP2(deck)}>+ IA</button>
                    </span>
                  </li>
                ))}
              </ul>
            </div>
            <div className="deck-selection-area">
              <div className="deck-selector">
                <h3>Deck Joueur 1 (toi)</h3>
                {deckP1 ? (
                  <div className="deck-item selected">
                    {deckP1.name} <span className="deck-count">{deckP1.cards.reduce((sum, c) => sum + c.quantity, 0)} cartes</span>
                    <button className="btn-remove" onClick={() => setDeckP1(null)}>-</button>
                  </div>
                ) : (
                  <div className="deck-item empty">Aucun deck sélectionné</div>
                )}
              </div>
              <div className="deck-selector">
                <h3>Deck Joueur 2 (IA)</h3>
                {deckP2 ? (
                  <div className="deck-item selected">
                    {deckP2.name} <span className="deck-count">{deckP2.cards.reduce((sum, c) => sum + c.quantity, 0)} cartes</span>
                    <button className="btn-remove" onClick={() => setDeckP2(null)}>-</button>
                  </div>
                ) : (
                  <div className="deck-item empty">Aucun deck sélectionné</div>
                )}
              </div>
            </div>
            <button className="btn" onClick={init} disabled={!deckP1 || !deckP2 || loading}>
              Démarrer la partie
            </button>
          </>
        )}

        {state && (
          <>
            <div className="magic-board">
              <PlayerZone
                playerId={state.playerTwoId}
                zones={enrichedZones} // Utiliser les zones enrichies
                isPlayable={false}
              />
              <div className="center-zone">
                <Library count={enrichedZones[`${state.playerTwoId}_library`]?.length || 0} />
                <div className="vs-label">VS</div>
                <Library count={enrichedZones[`${playerId}_library`]?.length || 0} />
              </div>
              <PlayerZone
                playerId={playerId}
                zones={enrichedZones} // Utiliser les zones enrichies
                onPlayCard={onPlay}
                isPlayable={state.activePlayerId === playerId}
              />
              <div className="game-status">
                <p>Joueur actif : {state.activePlayerId === playerId ? "Toi" : "IA"}</p>
                <p>Phase : {state.currentPhase}</p>
                <p>PV Toi : {state.players?.find(p => p.playerId === playerId)?.lifeTotal ?? 20}</p>
                <p>PV IA : {state.players?.find(p => p.playerId === state.playerTwoId)?.lifeTotal ?? 20}</p>
              </div>
              <GameActions actions={buildActions()} onAction={handleAction} />
              <button className="btn" onClick={refresh} disabled={loading}>Rafraîchir</button>
              {loading && <div>Chargement...</div>}
            </div>
          </>
        )}
      </div>
    </div>
  );
}