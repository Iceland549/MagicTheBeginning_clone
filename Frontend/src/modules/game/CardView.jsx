import React, { useState, useEffect } from 'react';
import CardModal from '../../components/CardModal';
import '../game-styles/CardView.css';

export default function CardView({ card, onPlay, onTap, disabled, playerId, currentPlayerId }) {
  const [selectedCard, setSelectedCard] = useState(null);
  const [localTapped, setLocalTapped] = useState(Boolean(card?.isTapped));

  useEffect(() => {
    setLocalTapped(Boolean(card?.isTapped));
  }, [card?.isTapped]);

  console.log("CardView render:", typeof card, card);

  if (!card) {
    console.error('CardView: card is null or undefined');
    return null;
  }

  if (!card.cardId) {
    console.error('CardView: card.cardId is undefined', card);
  }

  const isLand = card.typeLine && card.typeLine.toLowerCase().includes('land');

  const handleTapClick = async () => {
    console.log('[CardView] Tap click', { cardId: card.cardId, ownerId: card.ownerId, playerId, currentPlayerId });
    if (!onTap) return;
    try {
      await onTap(card.cardId); 
      setLocalTapped(true);
    } catch (err) {
      console.error('Tap failed', err);
      alert('Impossible de taper le terrain : ' + (err.message || err));
    }
  };

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
          className={localTapped ? 'tapped' : ''}
          onClick={() => setSelectedCard(card)} 
        />
      )}

      {isLand && onTap && playerId === currentPlayerId && (
        <>
          {!localTapped ? (
            <button className="btn" onClick={handleTapClick}>Tap</button>
          ) : (
            <span style={{ color: 'lightgreen' }}></span>
          )}
        </>
      )}

      {typeof onPlay === 'function' && (
        <button
          onClick={() => {
            if (!card.cardId) {
              console.error('CardView: Cannot play card, cardId is undefined');
              alert('Erreur : Carte sans ID');
              return;
            }
            onPlay(card.cardId);
          }}
          disabled={disabled || !card.cardId}
        >
          Jouer
        </button>
      )}

      {typeof card.power === 'number' && typeof card.toughness === 'number' && (
        <p>{card.power} / {card.toughness}</p>
      )}
      {localTapped && <p style={{ color: 'red' }}>TAP</p>}
      {card.hasSummoningSickness && <p style={{ color: 'orange' }}>Mal d'invocation</p>}

      <CardModal card={selectedCard} onClose={() => setSelectedCard(null)} />
    </div>
  );
}
