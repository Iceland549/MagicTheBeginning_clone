import React from 'react';

export default function CardView({ card, onPlay, disabled }) {
    console.log("CardView render:", card);

  if (!card) return null;

  return (
    <div className="card-view">
      <pre>{JSON.stringify(card, null, 2)}</pre>
      <h4>{card.name || card.cardId}</h4>
      <p>{card.typeLine}</p>

      {card.manaCost && <p>Coût : {card.manaCost}</p>}

      {card.imageUrl && (
        <img src={card.imageUrl} alt={card.name} style={{ maxWidth: '100px' }} />
      )}

      {typeof onPlay === 'function' && (
        <button onClick={() => onPlay(card.cardId)} disabled={disabled}>
          Jouer
        </button>
      )}

      {typeof card.power === 'number' && typeof card.toughness === 'number' && (
        <p>
          {card.power} / {card.toughness}
        </p>
      )}

      {card.isTapped && <p style={{ color: 'red' }}>TAP</p>}
      {card.hasSummoningSickness && <p style={{ color: 'orange' }}>Mal d'invocation</p>}
    </div>
  );
}
