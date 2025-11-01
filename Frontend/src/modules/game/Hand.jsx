import React, {useState} from 'react';
import CardView from './CardView';
import CardModal from '../../components/CardModal';
import Library from './Library';
import '../game-styles/Hand.css';

export default function Hand({ cards, onPlay, isPlayable, showControls, ...props }) {
  const [selectedCard, setSelectedCard] = useState(null);

  return (
    <div className="hand-zone">
      {showControls && (
        <div className="hand-header">
          <Library count={props.libraryCount} />
          <div className="vs-label">VS</div>
          <Library count={props.opponentLibraryCount} />
        </div>
      )}
      <div className="hand-cards">
        {(cards || [])
          .map((card, i) => (
            <CardView
              key={i}
              card={card}
              onPlay={onPlay}
              disabled={!isPlayable}
              onCardClick={setSelectedCard}
            />
          ))}
      </div>
      <CardModal card={selectedCard} onClose={() => setSelectedCard(null)} />
    </div>
  );
}
