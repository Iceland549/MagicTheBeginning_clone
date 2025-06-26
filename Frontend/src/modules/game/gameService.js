import api from '../../services/api';
export const startGame   = payload => api.post('/api/game/start', payload);
export const getGameState= id      => api.get(`/api/game/${id}`);
export const playCard    = (id, name) => api.post(`/api/game/${id}/play`, { cardName: name });