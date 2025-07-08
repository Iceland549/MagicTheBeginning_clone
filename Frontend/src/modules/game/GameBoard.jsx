import React, { useState, useEffect } from 'react';
import { startGame, getGameState, playCard } from './gameService';
import { getDecks } from '../decks/deckService';
import { useNavigate } from 'react-router-dom';
import './GameBoard.css'; // Assuming a CSS file for styling

export default function GameBoard() {
  const [gameId, setGameId] = useState('');
  const [state, setState] = useState(null);
  const [decks, setDecks] = useState([]);
  const [selectedDeckId, setSelectedDeckId] = useState(null);
  const navigate = useNavigate();
  const playerId = localStorage.getItem('userId'); // Get authenticated user ID

  // Fetch decks on component mount
  useEffect(() => {
    if (!playerId) {
      console.error('Player ID missing, user needs to login');
      navigate('/login');
      return;
    }

    getDecks(playerId)
      .then(response => {
        console.log('Decks fetched in GameBoard:', JSON.stringify(response.data, null, 2));
        setDecks(response.data || []);
      })
      .catch(error => {
        console.error('Error fetching decks:', error);
        setDecks([]);
      });
  }, [playerId, navigate]);

  const init = async () => {
    if (!selectedDeckId) {
      alert('Veuillez sélectionner un deck avant de démarrer la partie.');
      return;
    }

    try {
      const gameData = {
        playerOneId: playerId,
        playerTwoId: 'AI', // Assuming AI opponent for simplicity
        deckId: selectedDeckId // Include selected deck ID
      };
      const response = await startGame(gameData);
      setGameId(response.id);
      setState(response);
      console.log('Game started:', JSON.stringify(response, null, 2));
    } catch (error) {
      console.error('Error starting game:', error.response?.data || error.message);
      alert(`Erreur lors du démarrage de la partie : ${error.response?.data?.error || error.message}`);
    }
  };

  const refresh = async () => {
    if (!gameId) return;
    try {
      const response = await getGameState(gameId);
      setState(response);
      console.log('Game state refreshed:', JSON.stringify(response, null, 2));
    } catch (error) {
      console.error('Error refreshing game state:', error);
      alert('Erreur lors du rafraîchissement de l’état du jeu');
    }
  };

  const onPlay = async (cardId) => {
    try {
      await playCard(gameId, { cardId, type: 'PlayCard' });
      await refresh();
    } catch (error) {
      console.error('Error playing card:', error);
      alert('Erreur lors du jeu de la carte');
    }
  };

  const handleDeckSelect = (deckId) => {
    setSelectedDeckId(deckId);
    console.log('Selected deck ID:', deckId);
  };

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-game.jpg)' }}>
      <div className="container">
        <button className="btn-back" onClick={() => navigate('/dashboard')}>
          ← Retour au Dashboard
        </button>
        <h2>Plateau de jeu</h2>

        {/* Deck selection section */}
        <h2>Deck du joueur</h2>
        {decks.length > 0 ? (
          <ul className="deck-list">
            {decks.map(deck => (
              <li
                key={deck.id}
                className={`deck-item ${selectedDeckId === deck.id ? 'selected' : ''}`}
                onClick={() => handleDeckSelect(deck.id)}
              >
                {deck.name} - {deck.cards.reduce((sum, card) => sum + card.quantity, 0)} cartes
              </li>
            ))}
          </ul>
        ) : (
          <p>Aucun deck disponible. Veuillez créer un deck dans le constructeur.</p>
        )}

        {!state && (
          <button className="btn" onClick={init} disabled={!selectedDeckId}>
            Démarrer la partie
          </button>
        )}

        {state && (
          <>
            <p>Joueur actif : {state.activePlayerId}</p>
            <h3>Main</h3>
            <ul>
              {state.zones[`${state.activePlayerId}_hand`]?.map((card, i) => (
                <li key={i}>
                  {card.cardId} <button className="btn" onClick={() => onPlay(card.cardId)}>Jouer</button>
                </li>
              ))}
            </ul>
            <button className="btn" onClick={refresh}>Rafraîchir</button>
          </>
        )}
      </div>
    </div>
  );
}