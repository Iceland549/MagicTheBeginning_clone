import React, { useState } from 'react';
import { startGame, getGameState, playCard } from './gameService';
import { useNavigate } from 'react-router-dom';


export default function GameBoard() {
  const [gameId, setGameId] = useState('');
  const [state, setState]   = useState(null);
    const navigate = useNavigate();


  const init = async () => {
    const { data } = await startGame({ playerOneId: 'P1', playerTwoId: 'P2' });
    setGameId(data.id);
    setState(data);
  };

  const refresh = async () => {
    const { data } = await getGameState(gameId);
    setState(data);
  };

  const onPlay = async (name) => {
    await playCard(gameId, name);
    refresh();
  };

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-game.jpg)' }}>
      <div className="container">
        <button className="btn-back" onClick={() => navigate('/dashboard')}>
          ← Retour au Dashboard
        </button>
        <h2>Plateau de jeu</h2>
        {!state && <button className="btn" onClick={init}>Démarrer la partie</button>}
        {state && (
          <>
            <p>Joueur actif : {state.activePlayerId}</p>
            <h3>Main</h3>
            <ul>
              {state.zones[`${state.activePlayerId}_hand`].map((c,i) =>
                <li key={i}>
                  {c} <button className="btn" onClick={() => onPlay(c)}>Jouer</button>
                </li>
              )}
            </ul>
            <button className="btn" onClick={refresh}>Rafraîchir</button>
          </>
        )}
      </div>
    </div>
  );
}