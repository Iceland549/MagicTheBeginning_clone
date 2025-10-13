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
 * Fetch a card by Id.
 * GET /api/cards/{id}
 * @param {string} cardId
 * @returns {Promise<Object>} Card data
 */
export async function fetchCardById(cardId) {
  const { data } = await api.get(`/cards/${encodeURIComponent(cardId)}`);
  return data;
}

/**
 * Fetch a card by name, with optional set and language.
 * @param {string} name - Card name
 * @param {string} [set] - Optional set code (e.g., 'neo', 'sta')
 * @param {string} [lang] - Optional language (e.g., 'ja', 'fr')
 * @returns {Promise<Object>} Card data
 */
export async function fetchCardByName(name, set, lang, collectorNumber) {
  const params = {};
  if (set) params.set = set;
  if (lang) params.lang = lang;
  if (collectorNumber) params.collectorNumber = collectorNumber;


  const { data } = await api.get(`/cards/${encodeURIComponent(name)}`, { params });
  return data;
}

/**
 * Import a card from Scryfall by name, with optional set and language.
 * @param {string} name
 * @returns {Promise} Imported card data
 */
export async function importCard(name, set, lang, collectorNumber) {
  const params = {};
  if (set) params.set = set;
  if (lang) params.lang = lang;
  if (collectorNumber) params.collectorNumber = collectorNumber;

  const { data } = await api.post(`/cards/import/${encodeURIComponent(name)}`, null, { params });
  return data;
}

export async function deleteCardByName(cardId) {
  return api.delete(`/cards/${encodeURIComponent(cardId)}`);
}
