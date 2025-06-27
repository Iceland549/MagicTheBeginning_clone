import api from "../../services/api";

/**
 * Start a new game session.
 * @param {Object} gameData
 * @returns {Promise} Game session info
 */
export async function startGame(gameData) {
  const { data } = await api.post("/game/start", gameData);
  return data;
}

/**
 * Get the current state of a game session.
 * @param {string} gameId
 * @returns {Promise} Game state
 */
export async function getGameState(gameId) {
  const { data } = await api.get(`/game/${gameId}`);
  return data;
}

/**
 * Play a card in the current game session.
 * @param {string} gameId
 * @param {Object} playData - { cardName }
 * @returns {Promise} Updated game state
 */
export async function playCard(gameId, playData) {
  const { data } = await api.post(`/game/${gameId}/play`, playData);
  return data;
}