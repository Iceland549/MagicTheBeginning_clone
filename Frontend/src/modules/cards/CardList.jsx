import React, { useEffect, useState } from 'react';
import { getAllCards } from './cardService';

export default function CardList() {
  const [cards, setCards] = useState([]);

  useEffect(() => {
    getAllCards().then(r => setCards(r.data));
  }, []);

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-cards.jpg)' }}>
      <div className="container">
        <h2>Cartes</h2>
        <ul>
          {cards.map(c => (
            <li key={c.id}>
              <strong>{c.name}</strong> — {c.manaCost} — {c.typeLine}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}