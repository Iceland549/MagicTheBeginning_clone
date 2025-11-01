import React from 'react';
import Hand from './Hand';
import Graveyard from './Graveyard';
import '../game-styles/PlayerZone.css';

export default function PlayerZone({ playerId, zones, onPlayCard, isPlayable, isHuman }) {

  const handKey = Object.keys(zones).find(
  key => key.endsWith(`${playerId}_hand`)
  );
  console.log(" PlayerZone detected handKey:", handKey, "→", zones[handKey]);
  const graveyardKey = Object.keys(zones).find(
    key => key.toLowerCase().includes('graveyard') && key.toLowerCase().includes(playerId.toLowerCase())
  );

  return (
    <div className="player-zone">
      <h3>Joueur : {playerId}</h3>
      <Hand
        cards={zones[handKey] || []}
        onPlay={isHuman ? onPlayCard : undefined} 
        isPlayable={isPlayable}
        showControls={isHuman}
        playerId={playerId}
        libraryCount={zones[`${playerId}_library`]?.length || 0}          
       opponentLibraryCount={zones[`${isHuman ? 'AI' : 'Human'}_library`]?.length || 0} 
      />

      <Graveyard cards={zones[graveyardKey] || []} />
    </div>
  );
}
