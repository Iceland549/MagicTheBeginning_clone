import React, { useState } from 'react';
import { fetchCardByName } from './cardService';

export default function CardList() {
  const [search, setSearch] = useState('');
  const [card, setCard] = useState(null);

  const handleSearch = async () => {
    if (!search) return;
    try {
      const result = await fetchCardByName(search);
      setCard(result);
    } catch {
      alert('Carte non trouvée');
    }
  };

  return (
    <div className="card-list">
      <h2>Recherche de cartes</h2>
      <input
        value={search}
        onChange={e => setSearch(e.target.value)}
        placeholder="Nom de la carte"
      />
      <button onClick={handleSearch}>Rechercher</button>
      {card && (
        <div>
          <h3>{card.name}</h3>
          {card.imageUrl && <img src={card.imageUrl} alt={card.name} />}
        </div>
      )}
    </div>
  );
}
