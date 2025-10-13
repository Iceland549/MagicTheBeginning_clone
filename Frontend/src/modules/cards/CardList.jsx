import React, { useState } from 'react';
import { fetchCardByName } from './cardService';
import { useNavigate } from 'react-router-dom';
import { useSelection } from '../context/selectionContext';
import CardModal from '../../components/CardModal'; 


export default function CardList() {
  const [search, setSearch] = useState('');
  const [setCode, setSetCode] = useState('');
  const [lang, setLang] = useState('');
  const [card, setCard] = useState(null);
  const [collectorNumber, setCollectorNumber] = useState('');
  const [quantity, setQuantity] = useState(1); 
  const [feedback, setFeedback] = useState('');
  const navigate = useNavigate();
  const { addCard, removeCard } = useSelection();
  const [selectedCard, setSelectedCard] = useState(null);


  const handleSearch = async () => {
    try {
      const result = await fetchCardByName(search, setCode, lang, collectorNumber);
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
    setQuantity(1); 
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
        <div className="form-group mt-2">
          <label>Code du set (optionnel)</label>
          <input
            type="text"
            className="form-control"
            placeholder="Ex : sta, neo, one..."
            value={setCode}
            onChange={(e) => setSetCode(e.target.value)}
          />
        </div>
        <div className="form-group mt-2">
          <label>Numéro de collection (optionnel)</label>
          <input
            type="text"
            className="form-control"
            placeholder="Ex : 90, 44, 7a..."
            value={collectorNumber}
            onChange={(e) => setCollectorNumber(e.target.value)}
          />
        </div>
        <div className="form-group mt-2">
          <label>Langue (optionnel)</label>
          <input
            type="text"
            className="form-control"
            placeholder="Ex : ja, fr, de..."
            value={lang}
            onChange={(e) => setLang(e.target.value)}
          />
        </div>
        <button onClick={handleSearch}>Rechercher</button>
        {feedback && <p className="feedback">{feedback}</p>}
        {card && (
          <div className="card-result">
            <h3>{card.printedName || card.name}</h3>
            {card.imageUrl && (
              <img
                src={card.imageUrl}
                alt={card.name}
                width="200"
                onClick={() => setSelectedCard(card)}
                style={{ cursor: 'pointer' }}
              />
            )}
            <div>
              <label>Quantité : </label>
              <select
                value={quantity}
                onChange={(e) => setQuantity(parseInt(e.target.value, 10))}
              >
                {Array.from({ length: card.typeLine && card.typeLine.toLowerCase().includes('land') ? 20 : 4 }, (_, i) => i + 1).map(n => (
                  <option key={n} value={n}>{n}</option>
                ))}
              </select>
            </div>
            <button className="btn" onClick={handleAddToDeck}>
              Ajouter au deck
            </button>
                  <button
                    className="btn btn-remove"
                    onClick={() => {
                      removeCard(card);
                      setFeedback(`"${card.name}" a été supprimée du deck.`);
              }}
            >
              Supprimer du deck
            </button>
          </div>
        )}
      </div>
      <CardModal card={selectedCard} onClose={() => setSelectedCard(null)} />
    </div>
  );
}