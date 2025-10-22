import React, { useState, useEffect, useCallback } from 'react';
import { startGame, getGameState, playCard } from './gameService';
import { getAllDecks } from '../decks/deckService';
import { getAllCards } from '../cards/cardService'; 
import api from '../../services/api';
import { useNavigate } from 'react-router-dom';
import '../game-styles/GameBoard.css';
import Battlefield from './Battlefield';
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
  const [cardDetails, setCardDetails] = useState({}); 
  const navigate = useNavigate();
  const playerId = localStorage.getItem('userId'); 

  useEffect(() => {
    if (!playerId) {
      console.error('No playerId found in localStorage');
      navigate('/login');
      return;
    }
    console.log('playerId:', playerId);
    // Récupérer les decks
    getAllDecks()
      .then(response => setDecks(response.data || []))
      .catch(() => setDecks([]));

    // Récupérer toutes les cartes pour les détails
    getAllCards()
      .then(response => {
        console.log('Cards fetched in GameBoard:', JSON.stringify(response, null, 2));
        const cardMap = response.reduce((map, card) => {
          map[card.id] = card; 
          return map;
        }, {});
        setCardDetails(cardMap);
      })
      .catch(error => {
        console.error('Erreur lors de getAllCards:', error);
        setCardDetails({});
      });
  }, [playerId, navigate]);

  const isAITurn = state && state.activePlayerId === state.playerTwoId;

  const refresh = useCallback(async () => {
    if (!gameId) return;
    setLoading(true);
    try {
      const response = await getGameState(gameId);
      console.log('Game state:', JSON.stringify(response, null, 2));
      if (response?.zones) {
        console.log('Zones reçues du backend:', Object.keys(response.zones));
      } else {
        console.warn('Aucune zone reçue dans le game state');
      }
      setState({...response});
    } catch (error) {
      alert('Erreur lors du rafraîchissement de létat du jeu');
    }
    setLoading(false);
  }, [gameId]);

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
      console.log('=== ON PLAY ===');
      console.log('Current Phase:', state?.currentPhase);
      console.log('Action Type:', actionType);
      console.log('Card Name:', cardId);
    if (!gameId || !playerId) {
      console.error(`Invalid parameters: gameId=${gameId}, playerId=${playerId}`);
      alert('Erreur : paramètres manquants.');
      return;
    }

    if (['PlayCard', 'PlayLand'].includes(actionType) && !cardId) {
      console.error(`Invalid cardId for actionType=${actionType}`);
      alert('Erreur : aucune carte sélectionnée.');
      return;
    }

    setLoading(true);
    try {
      const playData = { PlayerId: playerId, CardId: cardId, Type: actionType };
      console.log('Sending playData:', JSON.stringify(playData, null, 2));
      await playCard(gameId, playData);
      await refresh();
    } catch (error) {
      console.error('Erreur lors de laction:', error);
      alert('Erreur lors de laction : ' + (error.message || ''));
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
      // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state?.activePlayerId, gameId, state?.playerTwoId, refresh]);

  const availableDecks = decks.filter(
    d => (!deckP1 || d.id !== deckP1.id) && (!deckP2 || d.id !== deckP2.id)
  );

  const buildActions = () => {
    if (!state || state.activePlayerId !== playerId) return [];

    const normalizeCard = (card) => ({
      ...card,
      cardId: card.cardId || card.CardId,
      name: card.name || card.Name,
    });

    console.log('=== BUILD ACTIONS ===');
    console.log('Current Phase:', state.currentPhase);
    console.log('Active Player:', state.activePlayerId);
    console.log('Player ID:', playerId);

    const hand = (state.zones[`${playerId}_hand`] || []).map(normalizeCard);
    const landCards = hand.filter(c => c.typeLine?.includes('Land'));
    const spellCards = hand.filter(c => !c.typeLine?.includes('Land'));

    const actions = [];

    switch (state.currentPhase) {
      case 'Draw':
        console.log('-> Adding Draw phase actions');
          actions.push({ label: 'Continuer', type: 'Draw' });

        break; 

      case 'Main':
        console.log('-> Adding Main phase actions');
        landCards.forEach(card => {
          actions.push({
            label: `Jouer Terrain: ${card.name}`,
            type: 'PlayLand',
            cardId: card.cardId 
          });
        });

        spellCards.forEach(card => {
          actions.push({
            label: `Jouer Sort: ${card.name}`,
            type: 'PlayCard',
            cardId: card.cardId 
          });
        });

        actions.push({ label: 'Passer à la phase de combat', type: 'PassToCombat' });
        actions.push({ label: 'Finir le tour', type: 'EndTurn' });
        break;

      case 'Combat':
        console.log('-> Adding Combat actions');
        actions.push({ label: 'Fin de combat', type: 'PreEnd' });
        break;

      case 'End':
        console.log('-> Adding End phase actions');
        actions.push({ label: 'Finir le tour', type: 'EndTurn' });
        break;

      default:
        console.warn('Unknown phase:', state.currentPhase);
        break;
    }

    console.log('Actions generated:', actions);
    return actions;
  };

  // Enrichir les zones avec les détails des cartes
  const enrichedZones = state
    ? Object.keys(state.zones).reduce((acc, zoneKey) => {
        acc[zoneKey] = state.zones[zoneKey].map(raw => {
          const ownerIdFromZone = zoneKey.split('_')[0]; // "playerId_battlefield" => playerId
          return {
            ...raw,
            cardId: raw.CardId || raw.cardId || raw.instanceId || null,
            name: raw.cardName || raw.name || raw.Name || '',
            typeLine: raw.typeLine || raw.TypeLine || '',
            imageUrl: raw.imageUrl || cardDetails[raw.cardId || raw.CardId || raw.cardName]?.imageUrl || 'https://via.placeholder.com/100',
            ownerId: raw.ownerId || raw.owner || raw.controllerId || ownerIdFromZone || null,
            isTapped: Boolean(raw.isTapped),
            CanBeTapped: Boolean(raw.CanBeTapped),
            hasSummoningSickness: Boolean(raw.hasSummoningSickness),
            ...raw
          };
        });
        return acc;
      }, {})
    : {};

  const handleTapLand = async (cardId, ownerIdFromUI) => {
      console.log('[GameBoard] handleTapLand called', { gameId, cardId, ownerIdFromUI, playerId }); 
    try {
      const playData = { PlayerId: playerId, CardId: cardId, Type: 'TapLand' };
      console.log('[GameBoard] Sending TapLand', playData);
      await playCard(gameId, playData);
      await refresh();
    } catch (error) {
      console.error('Erreur TapLand:', error);
    }
  };

    
  const handleAction = async (action) => {
    try {
      console.log("=== HANDLE ACTION ===");
      console.log("Action received:", action);

      if (!gameId || !playerId) {
        console.error("handleAction: gameId or playerId missing");
        alert("Erreur : paramètres manquants");
        return;
      }

      // Cas 1: Actions liées aux cartes (PlayCard, PlayLand)
      if (["PlayCard", "PlayLand"].includes(action.type)) {
        if (!action.cardId) {
          alert(`Erreur : aucune carte sélectionnée pour ${action.type}`);
          return;
        }
        await onPlay(action.cardId, action.type);
      } 
      // Cas 2: Actions simples (Draw, EndTurn, PassToCombat, PreEnd)
      else {
        const playData = { PlayerId: playerId, CardId: null, Type: action.type };
        console.log("Sending non-card action:", JSON.stringify(playData, null, 2));

      const updatedState = await playCard(gameId, playData);
      if (updatedState?.zones) {
        console.log("🟢 Nouveau gameState reçu directement:", updatedState);
        setState(updatedState);
      } else {
        console.warn("⚠️ Aucun gameState dans la réponse, fallback vers refresh()");
        await refresh();
      }

      }

    } catch (error) {
      console.error("Erreur lors de handleAction:", error);
      alert("Erreur lors de l'action : " + (error.message || "inconnue"));
    }
  };

  if (state) {
    console.log("♻️ Re-render GameBoard — Zones enrichies:", Object.keys(enrichedZones));
    console.log("🖐 Main joueur :", enrichedZones[`${playerId}_hand`]);
  }

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
          <div className="magic-board">
            <PlayerZone
              playerId={state.playerTwoId}
              zones={enrichedZones} 
              isPlayable={false}
            />

            <div className="battlefield-areas">
              <Battlefield
                cards={enrichedZones[`${state.playerTwoId}_battlefield`] || []}
                label={`Champ de bataille : ${state.playerTwoId}`}
                playerId={state.playerTwoId}
                currentPlayerId={playerId} 
              />
              <Battlefield
                cards={enrichedZones[`${playerId}_battlefield`] || []}
                label={`Champ de bataille : Toi`}
                  onTap={handleTapLand}
                  playerId={playerId}
                  currentPlayerId={playerId} 

              />
            </div>

            <PlayerZone
              playerId={playerId}
              zones={enrichedZones}
              onPlayCard={onPlay}
              isPlayable={state.activePlayerId === playerId && state.currentPhase === 'Main'}
              currentPhase={state.currentPhase}
            />

            <div className="center-zone">
              <Library count={enrichedZones[`${state.playerTwoId}_library`]?.length || 0} />
              <div className="vs-label">VS</div>
              <Library count={enrichedZones[`${playerId}_library`]?.length || 0} />
            </div>

            <div className="game-status">
              <p>Joueur actif : {state.activePlayerId === playerId ? "Toi" : "IA"}</p>
              <p>Phase : {state.currentPhase}</p>
              <p>PV Toi : {state.players?.find(p => p.playerId === playerId)?.lifeTotal ?? 20}</p>
              <p>PV IA : {state.players?.find(p => p.playerId === state.playerTwoId)?.lifeTotal ?? 20}</p>
            </div>
            <GameActions actions={buildActions()} onAction={handleAction} />
            {isAITurn && (
              <div className="ai-thinking-overlay">
                <img
                  src="/assets/ai-thinking.png"
                  alt="AI Thinking"
                  className="ai-thinking-image"
                />
                <div className="ai-thinking-text">L'adversaire joue...</div>
              </div>
          )}
          </div>
        )}
      </div>
    </div>
  );
}