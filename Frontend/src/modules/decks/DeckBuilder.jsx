import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; 
import { getAllCards } from '../cards/cardService';
import { createDeck, getDecks, validateDeck } from './deckService';
import { useSelection } from '../context/selectionContext';

export default function DeckBuilder() {
  const { selection, clearSelection, addCard, removeCard } = useSelection();
  const navigate = useNavigate();
  const [cards, setCards] = useState([]);
  const [deckName, setDeckName] = useState('');
  const ownerId = localStorage.getItem('userId'); 
  const [mesDecks, setMesDecks] = useState([]);


  useEffect(() => {
      getAllCards()
        .then(r => {
          console.log('Cards fetched in DeckBuilder:', r); 
          setCards(r || []);
        })
        .catch(error => {
          console.error('Erreur lors de getAllCards:', error);
          setCards([]);
        });
      getDecks(ownerId)
      .then(response => setMesDecks(response.data || []))
      .catch(error => {
        console.error("Erreur chargement decks", error);
        setMesDecks([]);
      });
  }, [ownerId]);


const handleCreate = async () => {
  const req = { ownerId, name: deckName, cards: selection };
  console.log('Creating deck with:', req); // Log de la requête
  const result = await validateDeck(req);
  console.log('Validation result:', result.data); // Log du résultat de validation
  const valid = result.data?.isValid ?? false;
  if (!valid) return alert('Deck invalide');
  const response = await createDeck(req);
  console.log('Deck creation response:', response); // Log de la réponse
  alert('Deck créé');
  clearSelection();
};

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-deck.jpg)' }}>
      <div className="container">
        <button className="btn-back" onClick={() => navigate('/dashboard')}>
          ← Retour au Dashboard
        </button>
        <h2>Constructeur de Deck</h2>
        <input value={deckName} onChange={e => setDeckName(e.target.value)} placeholder="Nom du deck" />
        <div className="deck-section">
          <h3>Mes decks</h3>
          {mesDecks.length > 0 ? (
            <ul className="deck-list">
              {mesDecks.map(deck => (
                <li key={deck.id} className="deck-item">
                  {deck.name} - {deck.cards.length} cartes
                </li>
              ))}
            </ul>
          ) : (
            <p>Aucun deck créé pour le moment</p>
          )}
        </div>
        <div>
          <h3>Cartes disponibles</h3>
           <ul>
          {cards.map(card => (
              <li key={card.name}>
                {card.name}
                {card.image_url && (
                  <img
                    src={card.image_url}
                    alt={card.name}
                    width="50"
                    onError={() => console.error('Image failed to load:', card.image_url)}
                  />
                )}
                <button
                  className="btn-add"
                  onClick={() => addCard(card)}
                >
                  +
                </button>
              </li>
            ))}
          </ul>
        </div>
        <div>
          <h3>Ma sélection</h3>
          <ul>
            {selection.map(c => (
              <li key={c.cardName}>
                {c.cardName} × {c.quantity}
                {c.image_url && (
                  <img
                    src={c.image_url}
                    alt={c.cardName}
                    width="100"
                    onError={(e) => console.error('Image failed to load:', c.image_url)}
                  />
                )}
                <button
                  className="btn-remove"
                  onClick={() => removeCard(c.cardName)}
                  style={{ marginLeft: '10px' }}
                > 
                  -
                </button>
              </li>
            ))}
          </ul>
        </div>
        <button className="btn" onClick={handleCreate}>Créer le deck</button>
      </div>
    </div>
  );
}
