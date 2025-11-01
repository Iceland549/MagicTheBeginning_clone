import React, {useState} from 'react';
import CardView from './CardView';
import CardModal from '../../components/CardModal';
import '../game-styles/Battlefield.css';

export default function Battlefield({
  cards,
  label,
  onTap,
  onTapCreature, 
  onSelectBlocker,
  playerId,
  currentPlayerId,
  currentPhase
}) {
  const spellCards = (cards || []).filter(card => !card.typeLine?.includes('Land') && !card.typeLine?.includes('Creature'));
  const creatureCards = (cards || []).filter(card => card.typeLine?.includes('Creature'));
  const landCards = (cards || []).filter(card => card.typeLine?.includes('Land'));
  const [selectedCard, setSelectedCard] = useState(null);

  return (
    <div className="battlefield-zone">
      <div className="battlefield-label">{label || 'Champ de bataille'}</div>

      {/* Sorts / Artefacts / Enchantements */}
      <div className="battlefield-cards-row">
        {spellCards.map((card, i) => (
          <CardView
            key={card.cardId || card.id || `${card.cardName}_${i}`}
            card={card}
            onCardClick={setSelectedCard}
          />
        ))}
      </div>

      {/* Créatures — Tap pendant Combat */}
      <div className="battlefield-cards-row">
        {creatureCards.map((card, i) => (
          <CardView
            key={card.cardId || card.id || `${card.cardName}_${i}`}
            card={card}
            onTapCreature={onTapCreature}
            onSelectBlocker={onSelectBlocker}
            playerId={playerId}
            currentPlayerId={currentPlayerId}
            currentPhase={currentPhase}
            onCardClick={setSelectedCard}
          />
        ))}
      </div>

      {/* Terrains — Tap pendant Main */}
      <div className="battlefield-cards-row">
        {landCards.map((card, i) => (
          <CardView
            key={card.cardId || card.id || `${card.cardName}_${i}`}
            card={card}
            onTap={onTap}
            playerId={playerId}
            currentPlayerId={currentPlayerId}
            currentPhase={currentPhase}
            onCardClick={setSelectedCard}
          />
        ))}
      </div>
      <CardModal card={selectedCard} onClose={() => setSelectedCard(null)} />
    </div>
  );
}
