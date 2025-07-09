import React, { useState, useEffect } from 'react';
import { startGame, getGameState, playCard } from './gameService';
import { getAllDecks } from '../decks/deckService';
import api from '../../services/api';
import { useNavigate } from 'react-router-dom';
import './GameBoard.css';

export default function GameBoard() {
  const [gameId, setGameId] = useState('');
  const [state, setState] = useState(null);
  const [decks, setDecks] = useState([]);
  const [deckP1, setDeckP1] = useState(null);
  const [deckP2, setDeckP2] = useState(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const playerId = localStorage.getItem('userId');

  // Récupération des decks
  useEffect(() => {
    if (!playerId) {
      navigate('/login');
      return;
    }
    getAllDecks()
      .then(response => setDecks(response.data || []))
      .catch(() => setDecks([]));
  }, [playerId, navigate]);

  // Rafraîchit la partie
  const refresh = async () => {
    if (!gameId) return;
    setLoading(true);
    try {
      const response = await getGameState(gameId);
      setState(response);
    } catch (error) {
      alert('Erreur lors du rafraîchissement de l’état du jeu');
    }
    setLoading(false);
  };

  // Démarre une partie
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

  // Joue une carte ou effectue une action
  const onPlay = async (cardId, actionType = 'PlayCard') => {
    if (!gameId) return;
    setLoading(true);
    try {
      await playCard(gameId, { cardId, type: actionType });
      await refresh();
    } catch (error) {
      alert('Erreur lors de l’action : ' + (error.message || ''));
    }
    setLoading(false);
  };

  // Appelle le tour de l'IA si c'est à elle de jouer
  useEffect(() => {
    if (
      state &&
      state.activePlayerId &&
      state.playerTwoId &&
      state.activePlayerId === state.playerTwoId
    ) {
      setLoading(true);
      api.post(`/games/${gameId}/ai-turn`)
        .then(() => refresh())
        .catch(() => setLoading(false));
    }
    // eslint-disable-next-line
  }, [state && state.activePlayerId]);

  // Decks non assignés
  const availableDecks = decks.filter(
    d => (!deckP1 || d.id !== deckP1.id) && (!deckP2 || d.id !== deckP2.id)
  );

  // Détermine l'action possible selon la phase
  const renderActions = () => {
    if (!state) return null;
    if (state.activePlayerId !== playerId) {
      return <div>En attente de l’IA...</div>;
    }
    switch (state.currentPhase) {
      case 'Draw':
        return (
          <button className="btn" onClick={refresh}>
            Piocher (rafraîchir)
          </button>
        );
      case 'Main':
        return (
          <>
            <div>
              <b>Jouer un terrain :</b>
              <ul>
                {(state.zones[`${playerId}_hand`] || [])
                  .filter(card => card.typeLine && card.typeLine.includes('Land'))
                  .map((card, i) => (
                    <li key={i} className="card-mini">
                      {card.cardId}
                      <button className="btn-play" onClick={() => onPlay(card.cardId, 'PlayLand')}>Jouer Terrain</button>
                    </li>
                  ))}
              </ul>
            </div>
            <div>
              <b>Jouer un sort :</b>
              <ul>
                {(state.zones[`${playerId}_hand`] || [])
                  .filter(card => !card.typeLine || !card.typeLine.includes('Land'))
                  .map((card, i) => (
                    <li key={i} className="card-mini">
                      {card.cardId}
                      <button className="btn-play" onClick={() => onPlay(card.cardId, 'PlayCard')}>Jouer Sort</button>
                    </li>
                  ))}
              </ul>
            </div>
            <button className="btn" onClick={() => onPlay(null, 'PassToCombat')}>Passer à la phase de combat</button>
            <button className="btn" onClick={() => onPlay(null, 'EndTurn')}>Finir le tour</button>
          </>
        );
      case 'Combat':
        return (
          <>
            <div>
              <b>Phase de combat :</b>
              {/* Pour l’instant, bouton pour finir la phase */}
              <button className="btn" onClick={() => onPlay(null, 'PreEnd')}>Fin de combat</button>
            </div>
          </>
        );
      case 'End':
        return (
          <button className="btn" onClick={() => onPlay(null, 'EndTurn')}>Finir le tour</button>
        );
      default:
        return null;
    }
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

        {state && state.zones && (
          <div className="magic-board">
            {/* Zone IA */}
            <div className="player-zone ai-zone">
              <h4>IA</h4>
              <div className="hand-zone">
                <span>Main IA :</span>
                <ul>
                  {(state.zones[`${state.playerTwoId}_hand`] || []).map((card, i) => (
                    <li key={i} className="card-mini">{card.cardId}</li>
                  ))}
                </ul>
              </div>
              <div className="battlefield-zone">
                <span>Champ de bataille IA</span>
                <ul>
                  {(state.zones[`${state.playerTwoId}_battlefield`] || []).map((card, i) => (
                    <li key={i} className="card-mini">{card.cardId}</li>
                  ))}
                </ul>
              </div>
              <div className="graveyard-zone">
                <span>Cimetière IA</span>
                <ul>
                  {(state.zones[`${state.playerTwoId}_graveyard`] || []).map((card, i) => (
                    <li key={i} className="card-mini">{card.cardId}</li>
                  ))}
                </ul>
              </div>
            </div>

            {/* Zone centrale */}
            <div className="center-zone">
              <div className="deck-pile">
                Deck IA : {state.zones[`${state.playerTwoId}_library`]?.length || 0}
              </div>
              <div className="vs-label">VS</div>
              <div className="deck-pile">
                Deck Joueur : {state.zones[`${playerId}_library`]?.length || 0}
              </div>
            </div>

            {/* Zone Joueur */}
            <div className="player-zone human-zone">
              <h4>Toi</h4>
              <div className="hand-zone">
                <span>Ta main :</span>
                <ul>
                  {(state.zones[`${playerId}_hand`] || []).map((card, i) => (
                    <li key={i} className="card-mini">
                      {card.cardId}
                      {/* Les boutons d'action sont dans renderActions */}
                    </li>
                  ))}
                </ul>
              </div>
              <div className="battlefield-zone">
                <span>Ton champ de bataille</span>
                <ul>
                  {(state.zones[`${playerId}_battlefield`] || []).map((card, i) => (
                    <li key={i} className="card-mini">{card.cardId}</li>
                  ))}
                </ul>
              </div>
              <div className="graveyard-zone">
                <span>Ton cimetière</span>
                <ul>
                  {(state.zones[`${playerId}_graveyard`] || []).map((card, i) => (
                    <li key={i} className="card-mini">{card.cardId}</li>
                  ))}
                </ul>
              </div>
            </div>

            {/* Infos de partie */}
            <div className="game-status">
              <p>Joueur actif : {state.activePlayerId === playerId ? "Toi" : "IA"}</p>
              <p>Phase : {state.currentPhase}</p>
              <p>PV Toi : {state.players?.find(p => p.playerId === playerId)?.lifeTotal ?? 20}</p>
              <p>PV IA : {state.players?.find(p => p.playerId === state.playerTwoId)?.lifeTotal ?? 20}</p>
            </div>

            {/* Actions dynamiques */}
            <div className="actions-zone">
              {renderActions()}
            </div>
            <button className="btn" onClick={refresh} disabled={loading}>Rafraîchir</button>
            {loading && <div>Chargement...</div>}
          </div>
        )}
      </div>
    </div>
  );
}
