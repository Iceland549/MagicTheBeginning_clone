import React from 'react';
import CardView from './CardView';
import Library from './Library';
import '../game-styles/Hand.css';

export default function Hand({ cards, onPlay, isPlayable, showControls, ...props }) {
  return (
    <div className="hand-zone">
      <h4>Ta main :</h4>
      {showControls && (
        <div className="center-zone">
          <Library count={props.libraryCount} />
          <div className="vs-label">VS</div>
          <Library count={props.opponentLibraryCount} />
        </div>
      )}
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
