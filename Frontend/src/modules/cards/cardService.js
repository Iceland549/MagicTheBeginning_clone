import api from "../../services/api";

/**
 * Fetch all cards.
 * @returns {Promise} List of cards
 */
export async function getAllCards() {
  const { data } = await api.get("/cards");
  return data;
}

/**
 * Fetch a card by name.
 * @param {string} name
 * @returns {Promise} Card data
 */
export async function fetchCardByName(name) {
  const { data } = await api.get('/api/cards', { params: { name } });
  return data;
}

/**
 * Import a card from Scryfall by name.
 * @param {string} name
 * @returns {Promise} Imported card data
 */
export async function importCard(name) {
  const { data } = await api.post(`/cards/import/${encodeURIComponent(name)}`);
  return data;
}