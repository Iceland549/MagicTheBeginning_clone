import api from '../../services/api';

export const login = async creds => {
  const { data } = await api.post('/auth/login', creds);
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  return data;
};

export const register = user => api.post('/account/register', user);
export const getProfile = () => api.get('/account/me');
export const sendConfirmation = email => api.post('/account/send-confirmation', email);
export const confirmEmail   = token => api.get(`/account/confirm?token=${token}`);