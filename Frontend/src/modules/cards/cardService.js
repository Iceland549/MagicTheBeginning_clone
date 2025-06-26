import api from '../../services/api';
export const getAllCards   = () => api.get('/api/cards');
export const getCardByName = name => api.get(`/api/cards/${name}`);
export const importCard    = name => api.post(`/api/cards/import/${name}`);