import React from 'react';
import Hand from './Hand';
import Graveyard from './Graveyard';
// import Library from './Library';
import '../game-styles/PlayerZone.css';

export default function PlayerZone({ playerId, zones, onPlayCard, isPlayable, isHuman }) {

  const handKey = Object.keys(zones).find(
  key => key.endsWith(`${playerId}_hand`)
  );
  console.log(" PlayerZone detected handKey:", handKey, "→", zones[handKey]);
  // const libraryKey = Object.keys(zones).find(
  //   key => key.toLowerCase().includes('library') && key.toLowerCase().includes(playerId.toLowerCase())
  // );
  const graveyardKey = Object.keys(zones).find(
    key => key.toLowerCase().includes('graveyard') && key.toLowerCase().includes(playerId.toLowerCase())
  );

  return (
    <div className="player-zone">
      <h3>Joueur : {playerId}</h3>

      {/* <Library count={zones[libraryKey]?.length || 0} /> */}

      <Hand
        cards={zones[handKey] || []}
        onPlay={onPlayCard}
        isPlayable={isPlayable}
        showControls={isHuman}
        playerId={playerId}
      />

      <Graveyard cards={zones[graveyardKey] || []} />
    </div>
  );
}
