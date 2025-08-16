import React from 'react';

function QuantitySelectorWithButton({ card, quantity, setQuantity, onCardAction, actionLabel }) {
  return (
    <div className="quantity-action">
      <select
        className="quantity-select"
        value={quantity}
        onChange={(e) => setQuantity(parseInt(e.target.value, 10))}
      >
        {/* Permettre jusqu'à 20 pour les terrains */}
        {Array.from({ length: 20 }, (_, i) => i + 1).map(n => (
          <option key={n} value={n}>{n}</option>
        ))}
      </select>
      <button
        className="btn-action"
        onClick={() => {
          console.log(`QuantitySelector: Clicking ${actionLabel} for card: ${card.name}, ID: ${card.id}, Quantity: ${quantity}`);
          onCardAction(card, quantity);
        }}
      >
        {actionLabel}
      </button>
    </div>
  );
}

export function CardGrid({
  title,
  cards,
  onCardAction,
  actionLabel,
  showQuantitySelector = false,
  quantities = {},
  setQuantities = () => {}
}) {
  return (
    <section className="deck-section">
      <h3>{title}</h3>
      <div className="card-grid">
        {cards.map(card => {
          const key = card.id;
          const quantity = quantities[key] || 1;
          console.log(`Rendering card: ${card.name}, ID: ${card.id}, TypeLine: ${card.typeLine || 'undefined'}`);
          return (
            <div key={key} className="card-item">
              {card.imageUrl && (
                <img
                  src={card.imageUrl}
                  alt={card.name || card.cardName}
                  className="card-thumb"
                  onError={() => console.error('Image failed to load:', card.imageUrl)}
                />
              )}
              <div className="card-info">
                <span className="card-name">{card.name || card.cardName}</span>
                {showQuantitySelector ? (
                  <QuantitySelectorWithButton
                    card={card}
                    quantity={quantity}
                    setQuantity={(qty) => setQuantities(prev => ({ ...prev, [key]: qty }))}
                    onCardAction={onCardAction}
                    actionLabel={actionLabel}
                  />
                ) : (
                  <div className="selection-info">
                    <span className="card-quantity">× {card.quantity || 1}</span>
                    <button className="btn-action" onClick={() => {
                      console.log(`CardGrid: Clicking ${actionLabel} for card: ${card.name}, ID: ${card.id}`);
                      onCardAction(card);
                    }}>
                      {actionLabel}
                    </button>
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}