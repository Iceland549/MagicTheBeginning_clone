import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000', // Ocelot Gateway
});

api.interceptors.request.use(cfg => {
  const token = localStorage.getItem('accessToken');
  if (token) cfg.headers.Authorization = `Bearer ${token}`;
  return cfg;
});

export default api;