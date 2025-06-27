import api from "../../services/api";

/**
 * Create a new deck.
 * @param {Object} deckData
 * @returns {Promise} Created deck
 */
export async function createDeck(deckData) {
  const { data } = await api.post("/decks", deckData);
  return data;
}

/**
 * Fetch all decks for a given owner.
 * @param {string} ownerId
 * @returns {Promise} List of decks
 */
export async function getDecks(ownerId) {
  const { data } = await api.get(`/decks/${ownerId}`);
  return data;
}

/**
 * Validate a deck.
 * @param {Object} deckData
 * @returns {Promise} Validation result
 */
export async function validateDeck(deckData) {
  const { data } = await api.post("/decks/validate", deckData);
  return data;
}