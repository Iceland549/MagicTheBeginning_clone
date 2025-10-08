import React, { useState } from 'react';
import CardModal from '../../components/CardModal';
import '../game-styles/CardView.css';

export default function CardView({ card, onPlay, disabled }) {
  const [selectedCard, setSelectedCard] = useState(null);

  console.log("CardView render:", typeof card, card);

  if (!card) {
    console.error('CardView: card is null or undefined');
    return null;
  }

  if (!card.cardName) {
    console.error('CardView: card.cardName is undefined', card);
  }

  return (
    <div className="card-view">
      <h4>{card.name || card.cardId || 'Carte sans ID'}</h4>
      <p>{card.typeLine}</p>
      {card.manaCost && <p>Coût : {card.manaCost}</p>}

      {card.imageUrl && (
        <img
          src={card.imageUrl || 'https://via.placeholder.com/100'}
          alt={card.name || 'Carte'}
          style={{ maxWidth: '100px', cursor: 'pointer' }}
          onClick={() => setSelectedCard(card)} // 👈 ouvre la modale
        />
      )}

      {typeof onPlay === 'function' && (
        <button
          onClick={() => {
            if (!card.cardName) {
              console.error('CardView: Cannot play card, cardName is undefined');
              alert('Erreur : Carte sans ID');
              return;
            }
            onPlay(card.cardName);
          }}
          disabled={disabled || !card.cardName}
        >
          Jouer
        </button>
      )}

      {typeof card.power === 'number' && typeof card.toughness === 'number' && (
        <p>{card.power} / {card.toughness}</p>
      )}
      {card.isTapped && <p style={{ color: 'red' }}>TAP</p>}
      {card.hasSummoningSickness && <p style={{ color: 'orange' }}>Mal d'invocation</p>}

      <CardModal card={selectedCard} onClose={() => setSelectedCard(null)} />
    </div>
  );
}
