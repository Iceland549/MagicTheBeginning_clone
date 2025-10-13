import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; 
import { getAllCards } from '../cards/cardService';
import { deleteCardByName } from '../cards/cardService'; 
import { createDeck, getDecksByOwner, validateDeck } from './deckService';
import { useSelection } from '../context/selectionContext';
import { CardGrid } from '../cardGrid/CardGrid';
import './DeckBuilder.css'; 

export default function DeckBuilder() {
  const { selection, clearSelection, addCard, removeCard } = useSelection();
  const navigate = useNavigate();
  const [cards, setCards] = useState([]);
  const [deckName, setDeckName] = useState('');
  const ownerId = localStorage.getItem('userId'); 
  const [mesDecks, setMesDecks] = useState([]);
  const [quantities, setQuantities] = useState({});

  useEffect(() => {
    getAllCards()
      .then(r => {
        console.log('Cards fetched in DeckBuilder:', JSON.stringify(r, null, 2));
        setCards(r || []);
      })
      .catch(error => {
        console.error('Erreur lors de getAllCards:', error);
        setCards([]);
      });

    if (!ownerId) {
      console.error('Owner ID missing, user needs to login');
      navigate('/login');
      return;
    }

    getDecksByOwner(ownerId)
      .then(response => {
        console.log('Decks fetched:', JSON.stringify(response.data, null, 2));
        setMesDecks(response.data || []);
      })
      .catch(error => {
        console.error("Erreur chargement decks", error);
        setMesDecks([]);
      });
  }, [ownerId, navigate]);

  const validateDeckLocally = () => {
    console.log('Validating deck locally with selection:', JSON.stringify(selection, null, 2));
    if (!deckName.trim()) {
      return { isValid: false, error: 'Le nom du deck est requis' };
    }

    if (selection.length === 0) {
      return { isValid: false, error: 'Aucune carte sélectionnée' };
    }

    const totalCards = selection.reduce((sum, card) => sum + (card.quantity || 1), 0);
    if (totalCards < 60) {
      return { isValid: false, error: `Le deck doit contenir au moins 60 cartes, actuellement ${totalCards}` };
    }

    let landCount = 0;
    for (const card of selection) {
      const quantity = card.quantity || 1;
      console.log(`Checking card: ${card.name}, ID: ${card.id}, Quantity: ${quantity}, TypeLine: ${card.typeLine || 'undefined'}`);
      const isLand = card.typeLine && card.typeLine.toLowerCase().includes('land');
      if (quantity > 4 && !isLand) {
        return { isValid: false, error: `La carte ${card.name} dépasse la limite de 4 exemplaires (actuellement ${quantity})` };
      }
      if (isLand) {
        landCount += quantity;
      }
    }

    if (landCount < 20) {
      return { isValid: false, error: `Le deck doit contenir au moins 20 terrains, actuellement ${landCount}` };
    }

    return { isValid: true, error: '' };
  };

  const handleCreate = async () => {
    if (!ownerId) {
      alert('Erreur : utilisateur non connecté');
      return;
    }

    const localValidation = validateDeckLocally();
    if (!localValidation.isValid) {
      console.log('Local validation failed:', localValidation.error);
      alert(`Erreur : ${localValidation.error}`);
      return;
    }

    const req = {
      OwnerId: ownerId,
      Name: deckName,
      Cards: selection.map(card => ({
        CardId: card.id,
        Quantity: card.quantity || 1
      }))
    };

    console.log('Creating deck with:', JSON.stringify(req, null, 2));
    try {
      const result = await validateDeck(req);
      console.log('Validation result:', JSON.stringify(result.data, null, 2));
      const valid = result.data?.isValid ?? false;
      if (!valid) {
        const errorMessage = result.data?.error || 'Deck invalide : vérifiez les cartes et les quantités';
        console.log('Server validation failed:', errorMessage);
        alert(errorMessage);
        return;
      }

      const response = await createDeck(req);
      console.log('Deck creation response:', JSON.stringify(response, null, 2));
      alert('Deck créé avec succès');
      clearSelection();
      setDeckName('');

      const updatedDecks = await getDecksByOwner(ownerId);
      console.log('Updated decks:', JSON.stringify(updatedDecks.data, null, 2));
      setMesDecks(updatedDecks.data || []);
    } catch (error) {
      console.error('Error creating deck:', error.response?.data || error.message);
      alert(`Erreur lors de la création du deck : ${error.response?.data?.error || error.message}`);
    }
  };

  const handleDeleteAvailableCard = async (card) => {
    try {
      await deleteCardByName(card.name);
      setCards(prev => prev.filter(c => c.name !== card.name)); 
      removeCard(card); 
    } catch (e) {
      alert("Erreur lors de la suppression : " + (e.response?.data || e.message));
    }
  };

  console.log('Current selection:', JSON.stringify(selection, null, 2));

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

        <CardGrid
          title="Cartes disponibles"
          cards={cards}
          onCardAction={(card, qty) => {
            console.log(`Adding card: ${card.name}, ID: ${card.id}, Quantity: ${qty}`);
            addCard(card, qty);
          }}
          actionLabel="+"
          showQuantitySelector={true}
          quantities={quantities}
          setQuantities={setQuantities}
          onDeleteCard={handleDeleteAvailableCard}
        />

        <CardGrid
          title="Ma sélection"
          cards={selection}
          onCardAction={(card) => {
            console.log(`Removing card: ${card.name}, ID: ${card.id}`);
            removeCard(card);
          }}
          actionLabel="-"
        />

        <button className="btn" onClick={handleCreate}>Créer le deck</button>
      </div>
    </div>
  );
}
