import api from "../../services/api";

/**
 * Create a new deck (POST /api/decks).
 * @param {Object} deck
 * @returns {Promise} Created deck
 */
export const createDeck = async (deck) => {
  const formattedDeck = {
    OwnerId: deck.OwnerId,
    Name: deck.Name,
    Cards: deck.Cards.map(card => ({
      CardId: card.CardId,  
      Quantity: card.Quantity
    }))
  };
  console.log('Sending create request:', JSON.stringify(formattedDeck, null, 2));
  try {
    const response = await api.post('/decks', formattedDeck);
    return response.data;
  } catch (error) {
    console.error('Create deck error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
};

/**
 * Fetch all decks for a given owner.
 * @param {string} ownerId
 * @returns {Promise} List of decks
 */
export const getDecksByOwner = async (ownerId) => {
  try {
    const response = await api.get(`/decks/owner/${encodeURIComponent(ownerId)}`);
    return response;
  } catch (error) {
    console.error('Get decks error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
};

/**
 * Validate a deck.
 * @param {Object} deck
 * @returns {Promise} Validation result
 */
export const validateDeck = async (deck) => {
  const formattedDeck = {
    OwnerId: deck.OwnerId,
    Name: deck.Name,
    Cards: deck.Cards.map(card => ({
      CardId: card.CardId,
      Quantity: card.Quantity
    }))
  };
  console.log('Sending validate request:', JSON.stringify(formattedDeck, null, 2));
  try {
    const response = await api.post('/decks/validate', formattedDeck);
    return response;
  } catch (error) {
    console.error('Validate deck error:', error.response?.data || error.message);
    throw new Error(error.response?.data?.Error || 'Failed to validate deck');
  }
};

export function getAllDecks() {
  return api.get('/decks/all');
}

/**
 * Get deck by id (GET /api/decks/id/{id}).
 * @param {string} id
 * @returns {Promise}
 */
export const getDeckById = async (id) => {
  const response = await api.get(`/decks/${encodeURIComponent(id)}`);
  return response.data;
};

/**
 * Check if card exists in any deck (GET /api/decks/exists-card/{cardId}).
 * @param {string} cardId
 * @returns {Promise<boolean>}
 */
export function existsCardInAnyDeck(cardId) {
  return api.get(`/decks/exists-card/${encodeURIComponent(cardId)}`).then(r => r.data);
}

/**
 * Fetch all available cards from DeckMicroservice (which proxies CardMicroservice)
 * @returns {Promise<Array>} List of available cards
 */
export const fetchAvailableCards = async () => {
  try {
    const response = await api.get('/decks/available-cards');
    return response.data;
  } catch (error) {
    console.error('Fetch available cards error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
};
