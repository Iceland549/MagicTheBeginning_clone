import React from 'react';
import CardView from './CardView';

export default function Hand({ cards, onPlay, isPlayable }) {
  return (
    <div className="hand-zone">
      <h4>Ta main :</h4>
      <div className="card-list">
        {(cards || []).map((card, i) => (
          <CardView
            key={i}
            card={card}
            onPlay={onPlay}
            disabled={!isPlayable}
          />
        ))}
      </div>
    </div>
  );
}
