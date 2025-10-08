import React from 'react';
import CardView from './CardView';
import '../game-styles/Battlefield.css';

export default function Battlefield({ cards, label }) {
  const spellCards = (cards || []).filter(card => !card.typeLine?.includes('Land'));
  const landCards = (cards || []).filter(card => card.typeLine?.includes('Land'));

  return (
    <div className="battlefield-zone">
      <div className="battlefield-label">
        {label || 'Champ de bataille'}
      </div>
      <div className="battlefield-cards-row">
        {spellCards.map((card, i) => (
          <CardView key={card.id || `${card.cardName}_${i}`} card={card} />
        ))}
      </div>
      <div className="battlefield-cards-row">
        {landCards.map((card, i) => (
          <CardView key={card.id || `${card.cardName}_${i}`} card={card} />
        ))}
      </div>
    </div>
  );
}