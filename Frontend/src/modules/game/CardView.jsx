import React, { useState, useEffect } from 'react';
import CardModal from '../../components/CardModal';
import '../game-styles/CardView.css';

export default function CardView({
  card,
  onPlay,
  onTap,
  onTapCreature, 
  onSelectBlocker,
  disabled,
  playerId,
  currentPlayerId,
  currentPhase
}) {
  const [selectedCard, setSelectedCard] = useState(null);
  const [localTapped, setLocalTapped] = useState(Boolean(card?.isTapped));
  const [isBlocking, setIsBlocking] = useState(false);

  useEffect(() => {
    setLocalTapped(Boolean(card?.isTapped));
  }, [card?.isTapped]);

  if (!card) return null;
  if (!card.cardId) console.error('CardView: card.cardId is undefined', card);

  const isLand = card.typeLine?.toLowerCase().includes('land');
  const isCreature = card.typeLine?.toLowerCase().includes('creature');
  
  const handleTapClick = async () => {
    console.log('[CardView] Tap click', { cardId: card.cardId, playerId, currentPlayerId });
    if (!onTap) return;
    try {
      await onTap(card.cardId);
      setLocalTapped(true);
    } catch (err) {
      console.error('Tap failed', err);
      alert('Impossible de taper le terrain : ' + (err.message || err));
    }
  };

  const handleTapCreatureClick = () => {
    if (currentPhase !== 'Combat' || playerId !== currentPlayerId) return;
    console.log('[CardView] Tap local (combat phase)', { cardId: card.cardId });

    // toggle visuel
    setLocalTapped(prev => {
      const newValue = !prev;
      if (typeof onTapCreature === 'function') {
        onTapCreature(card.cardId, newValue);
      }
      return newValue;
    });
  };

  const handleBlockCreatureClick = (attackerId) => {
    if (currentPhase !== 'Combat' || playerId !== currentPlayerId) return;
    setIsBlocking(prev => {
      const newValue = !prev;
      if (typeof onSelectBlocker === 'function') {
        onSelectBlocker(attackerId, card.cardId, newValue);
      }
      return newValue;
    });
  };

  return (
    <div
      className={`card-view ${card.isTapped ? 'card-tapped' : ''}`}
      onClick={() => setSelectedCard(card)}
    >
      {card.imageUrl && (
        <img
          src={card.imageUrl || 'https://via.placeholder.com/100'}
          alt={card.name || 'Carte'}
          style={{ maxWidth: '100px', cursor: 'pointer' }}
          className={`battle-card-img${card.isTapped ? ' tapped' : ''}${card.isAttacking ? ' creature-attacking' : ''}`}
        />
      )}

      {/* Tap des Terrains */}
      {isLand && onTap && playerId === currentPlayerId && (
        <>
          {!localTapped ? (
            <button className="btn" onClick={handleTapClick}>Tap</button>
          ) : (
            <span></span>
          )}
        </>
      )}

      {isCreature && playerId === currentPlayerId && currentPhase === 'Combat' && !card.hasSummoningSickness && (
        <button className="btn-tap" onClick={handleTapCreatureClick}>
          {localTapped ? 'Untap' : 'Tap '}
        </button>
      )}

      {/* Sélection des bloqueurs pendant la phase de blocage */}
      {isCreature && currentPhase === 'Combat' && card.canBlock && (
        <button
          className="btn-block"
          onClick={() => handleBlockCreatureClick(card.attackerTargetId)}
        >
      {isBlocking ? 'Unblock' : 'Block'}
        </button>
      )}


      {/* Bouton "Jouer" si applicable */}
      {typeof onPlay === 'function' && (
        <button
          onClick={() => {
            if (!card.cardId) {
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

      {/* Infos supplémentaires
      {typeof card.power === 'number' && typeof card.toughness === 'number' && (
        // <p>{card.power} / {card.toughness}</p>
      )} */}
      {localTapped}
      {card.hasSummoningSickness && <p style={{ color: 'orange' }}>Mal d’invocation</p>}

      <CardModal card={selectedCard} onClose={() => setSelectedCard(null)} />
    </div>
  );
}
