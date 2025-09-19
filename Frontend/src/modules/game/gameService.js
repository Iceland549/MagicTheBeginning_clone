import api from '../../services/api';

/**
 * Start a new game session.
 * @param {Object} gameData - { playerOneId, playerTwoId, deckId }
 * @returns {Promise} Game session info
 */
export async function startGame(gameData) {
  try {
    console.log('Starting game with data:', JSON.stringify(gameData, null, 2));
    const { data } = await api.post('/games/start', gameData);
    return data;
  } catch (error) {
    console.error('Start game error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
}

/**
 * Get the current state of a game session.
 * @param {string} gameId
 * @returns {Promise} Game state
 */
export async function getGameState(gameId) {
  try {
    const { data } = await api.get(`/games/${gameId}`);
    return data;
  } catch (error) {
    console.error('Get game state error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
}

/**
 * Play a card or perform an action in the current game session.
 * @param {string} gameId
 * @param {Object} playData - { cardId, type, targetId, attackers, blockers }
 * @returns {Promise} Updated game state
 */
export async function playCard(gameId, playData) {
  try {
    if (!playData || typeof playData !== 'object') {
      throw new Error('playData is invalid or empty');
    }
    const { PlayerId, CardId, Type } = playData;
    if (!PlayerId || !CardId || !Type) {
      throw new Error(`Invalid playData: PlayerId=${PlayerId}, CardId=${CardId}, Type=${Type}`);
    }
    console.log('Playing action:', JSON.stringify({ PlayerId, CardId, Type }, null, 2));
    const { data } = await api.post(`/games/${gameId}/action`, { PlayerId, CardId, Type });
    return data;
  } catch (error) {
    console.error('Play card error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
}