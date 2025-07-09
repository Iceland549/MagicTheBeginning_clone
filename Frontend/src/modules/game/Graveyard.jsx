import React from 'react';
import CardView from './CardView';

export default function Graveyard({ cards }) {
  return (
    <div className="graveyard-zone">
      <h4>Cimetière :</h4>
      <div className="card-list">
        {(cards || []).map((card, i) => (
          <CardView key={i} card={card} />
        ))}
      </div>
    </div>
  );
}