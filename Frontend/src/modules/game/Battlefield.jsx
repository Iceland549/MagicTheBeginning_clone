import React from 'react';
import CardView from './CardView';
import '../game-styles/Battlefield.css';

export default function Battlefield({ cards, label, onTap, playerId, currentPlayerId }) {
  const spellCards = (cards || []).filter(card => !card.typeLine?.includes('Land'));
  const landCards = (cards || []).filter(card => card.typeLine?.includes('Land'));

  return (
    <div className="battlefield-zone">
      <div className="battlefield-label">
        {label || 'Champ de bataille'}
      </div>

      {/* Sorts */}
      <div className="battlefield-cards-row">
        {spellCards.map((card, i) => (
          <CardView
            key={card.cardId || card.id || `${card.cardName}_${i}`}
            card={card}
          />
        ))}
      </div>

      {/* Terrains (Tap activable) */}
      <div className="battlefield-cards-row">
        {landCards.map((card, i) => (
          <CardView
            key={card.cardId || card.id || `${card.cardName}_${i}`}
            card={card}
            onTap={onTap}
            playerId={playerId}
            currentPlayerId={currentPlayerId} 
          />
        ))}
      </div>
    </div>
  );
}
