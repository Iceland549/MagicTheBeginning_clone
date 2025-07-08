import React, { useState } from 'react';
import { fetchCardByName } from './cardService';
import { useNavigate } from 'react-router-dom';
import { useSelection } from '../context/selectionContext';

export default function CardList() {
  const [search, setSearch] = useState('');
  const [card, setCard] = useState(null);
  const [quantity, setQuantity] = useState(1); // Ajout d'un état pour la quantité
  const [feedback, setFeedback] = useState('');
  const navigate = useNavigate();
  const { addCard } = useSelection();

  const handleSearch = async () => {
    if (!search) return;
    try {
      const result = await fetchCardByName(search);
      console.log('Card data fetched:', JSON.stringify(result, null, 2));
      setCard(result);
      setFeedback('');
    } catch (error) {
      console.error('Error fetching card:', error);
      alert('Carte non trouvée');
      setCard(null);
    }
  };

  const handleAddToDeck = () => {
    if (!card) return;
    console.log('Adding card to deck:', JSON.stringify(card, null, 2), 'Quantity:', quantity);
    addCard(card, quantity);
    setFeedback(`La carte "${card.name}" (x${quantity}) a été ajoutée à ton deck !`);
    setQuantity(1); // Réinitialiser la quantité après ajout
  };

  return (
    <div className="section" style={{
      backgroundImage: `url(${process.env.PUBLIC_URL}/assets/bg-cards.jpg)`
    }}>
      <div className="container">
        <button className="btn-back" onClick={() => navigate('/dashboard')}>
          ← Retour au Dashboard
        </button>
        <h2>Recherche de cartes</h2>
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Nom de la carte"
        />
        <button onClick={handleSearch}>Rechercher</button>
        {feedback && <p className="feedback">{feedback}</p>}
        {card && (
          <div className="card-result">
            <h3>{card.name}</h3>
            {card.image_url && (
              <img
                src={card.image_url}
                alt={card.name}
                width="200"
                onError={(e) => console.error('Image failed to load:', card.image_url)}
              />
            )}
            <div>
              <label>Quantité : </label>
              <select
                value={quantity}
                onChange={(e) => setQuantity(parseInt(e.target.value, 10))}
              >
                {Array.from({ length: card.type_line && card.type_line.toLowerCase().includes('land') ? 20 : 4 }, (_, i) => i + 1).map(n => (
                  <option key={n} value={n}>{n}</option>
                ))}
              </select>
            </div>
            <button className="btn" onClick={handleAddToDeck}>
              Ajouter au deck
            </button>
          </div>
        )}
      </div>
    </div>
  );
}