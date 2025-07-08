import api from "../../services/api";

/**
 * Create a new deck.
 * @param {Object} deck
 * @returns {Promise} Created deck
 */
export const createDeck = async (deck) => {
  const formattedDeck = {
    OwnerId: deck.OwnerId,
    Name: deck.Name,
    Cards: deck.Cards.map(card => ({
      CardName: card.CardName,
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
export const getDecks = async (ownerId) => {
  try {
    const response = await api.get(`/decks/${ownerId}`);
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
      CardName: card.CardName,
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