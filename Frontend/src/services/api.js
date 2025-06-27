import axios from 'axios';

// Vérification de l'URL de base de l'API
// Si l'URL n'est pas définie, on utilise une valeur par défaut
const API_BASE_URL = process.env.REACT_APP_API_URL || "http://localhost:5000/api";

// Création d'une instance axios avec intercepteur JWT
const api = axios.create({
  baseURL: API_BASE_URL,
});


api.interceptors.request.use(cfg => {
  const token = localStorage.getItem('accessToken');
  if (token) cfg.headers.Authorization = `Bearer ${token}`;
  return cfg;
},
(error) => Promise.reject(error));

export default api;