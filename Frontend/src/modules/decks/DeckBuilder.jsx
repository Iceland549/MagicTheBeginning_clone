import React, { useState, useEffect } from 'react';
import { getAllCards } from '../cards/cardService';
import { createDeck, getDecks, validateDeck } from './deckService';

export default function DeckBuilder() {
  const [cards, setCards]     = useState([]);
  const [deckName, setDeckName] = useState('');
  const [ownerId]            = useState(localStorage.getItem('userId'));
  const [selection, setSelection] = useState([]);

  useEffect(() => {
    getAllCards().then(r => setCards(r.data));
    getDecks(ownerId).then(r => console.log('Mes decks', r.data));
  }, [ownerId]);

  const addCard = (card) => {
    const found = selection.find(c => c.cardName === card.name);
    if (found) found.quantity++;
    else selection.push({ cardName: card.name, quantity: 1 });
    setSelection([...selection]);
  };

  const handleCreate = async () => {
    const req = { ownerId, name: deckName, cards: selection };
    const valid = (await validateDeck(req)).data.isValid;
    if (!valid) return alert('Deck invalide');
    await createDeck(req);
    alert('Deck créé');
  };

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-deck.jpg)' }}>
      <div className="container">
        <h2>Constructeur de Deck</h2>
        <input value={deckName} onChange={e => setDeckName(e.target.value)} placeholder="Nom du deck" />
        <div>
          <h3>Cartes disponibles</h3>
          {cards.map(c => (
            <div key={c.id}>
              {c.name} <button className="btn" onClick={() => addCard(c)}>+</button>
            </div>
          ))}
        </div>
        <div>
          <h3>Ma sélection</h3>
          <ul>
            {selection.map(c => (
              <li key={c.cardName}>
                {c.cardName} × {c.quantity}
              </li>
            ))}
          </ul>
        </div>
        <button className="btn" onClick={handleCreate}>Créer le deck</button>
      </div>
    </div>
  );
}