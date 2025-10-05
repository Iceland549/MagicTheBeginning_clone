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
    if (data.accessToken) {
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('expiresAt', data.expiresAt);
    }
    return data;
  } catch (error) {
    console.error('Start game error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
}

export async function refreshToken() {
  try {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) throw new Error('No refresh token available');
    const { data } = await api.post('/auth/refresh', { refreshToken });
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('expiresAt', data.expiresAt);
    localStorage.setItem('userId', data.userId);
    console.log('Token refreshed:', { userId: data.userId, expiresAt: data.expiresAt });
    return data.accessToken;
  } catch (error) {
    console.error('Token refresh failed:', error.response?.data || error.message);
    localStorage.clear();
    window.location.href = '/login';
    throw error;
  }
}

/**
 * Get the current state of a game session.
 * @param {string} gameId
 * @returns {Promise} Game state
 */
export async function getGameState(gameId) {
  try {
    const token = localStorage.getItem('accessToken');
    const userId = localStorage.getItem('userId');
    console.log('Fetching game state for gameId:', gameId, 'with token:', token ? 'present' : 'missing', 'userId:', userId);
    const { data } = await api.get(`/games/${gameId}`);
    return data;
  } catch (error) {
    console.error('Get game state error:', error.response?.data || error.message);
    if (error.response?.status === 401) {
      console.error('Authentication failed. Attempting token refresh...');
      try {
        await refreshToken();
        const { data } = await api.get(`/games/${gameId}`); // Retry
        return data;
      } catch (refreshError) {
        console.error('Token refresh failed, redirecting to login');
        localStorage.clear();
        window.location.href = '/login';
        throw refreshError;
      }
    }
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
    // Accept both PascalCase and camelCase to be robust
    const PlayerId = playData.PlayerId || playData.playerId || localStorage.getItem('userId');
    const CardName = playData.CardName || playData.cardName || null;
    const Type = playData.Type || playData.type || null;
    if (!PlayerId || !Type) {
      throw new Error(`Invalid playData: PlayerId=${PlayerId}, CardId=${CardName}, Type=${Type}`);
    }
    // For PlayCard/PlayLand we must have CardId defined
    if ((Type === 'PlayCard' || Type === 'PlayLand') && !CardName) {
      throw new Error(`Action ${Type} requires CardId (received: ${CardName})`);
    }
    console.log('Playing action:', JSON.stringify({ PlayerId, CardName, Type }, null, 2));
    const { data } = await api.post(`/games/${gameId}/action`, { PlayerId, CardName, Type });
    return data;
  } catch (error) {
    console.error('Play card error:', error.response?.data || error.message);
    throw error.response?.data || error;
  }
}