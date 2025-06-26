import api from '../../services/api';
export const createDeck   = deck => api.post('/api/decks', deck);
export const getDecks     = ownerId => api.get(`/api/decks/${ownerId}`);
export const validateDeck = deck => api.post('/api/decks/validate', deck);