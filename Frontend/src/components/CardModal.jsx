import React from 'react';
import './CardModal.css'; 

export default function CardModal({ card, onClose }) {
  if (!card) return null; 

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-btn" onClick={onClose}>×</button>
        <h2>{card.name}</h2>
        {card.imageUrl ? (
          <img src={card.imageUrl} alt={card.name} className="card-large" />
        ) : (
          <p>Image non disponible</p>
        )}
        {card.typeLine && <p><strong>Type :</strong> {card.typeLine}</p>}
        {card.oracleText && <p><strong>Texte :</strong> {card.oracleText}</p>}
      </div>
    </div>
  );
}
