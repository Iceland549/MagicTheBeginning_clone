import React from 'react';
import CardView from './CardView';

export default function Battlefield({ cards }) {
  return (
    <div className="battlefield-zone">
      <h4>Champ de bataille :</h4>
      <div className="card-list">
        {(cards || []).map((card, i) => (
          <CardView key={i} card={card} />
        ))}
      </div>
    </div>
  );
}