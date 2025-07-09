import React from 'react';
import Hand from './Hand';
import Battlefield from './Battlefield';
import Graveyard from './Graveyard';
import Library from './Library';

export default function PlayerZone({ playerId, zones, onPlayCard, isPlayable }) {
  return (
    <div className="player-zone">
      <h3>Joueur : {playerId}</h3>
      <Library count={zones[`${playerId}_library`]?.length || 0} />
      <Hand
        cards={zones[`${playerId}_hand`] || []}
        onPlay={onPlayCard}
        isPlayable={isPlayable}
      />
      <Battlefield cards={zones[`${playerId}_battlefield`] || []} />
      <Graveyard cards={zones[`${playerId}_graveyard`] || []} />
    </div>
  );
}