import api from "../../services/api";

/**
 * Log in a user and store access/refresh tokens in localStorage.
 * @param {Object} credentials - { userName, password }
 * @returns {Promise} Login response data
 */
export async function login(credentials) {
  const { data } = await api.post("/auth/login", credentials);
  localStorage.setItem("accessToken", data.accessToken);
  localStorage.setItem("refreshToken", data.refreshToken);
  return data;
}

export async function logout() {
  const refreshToken = localStorage.getItem('refreshToken');
  if (refreshToken) {
    await api.post('/auth/logout', { refreshToken });
  }
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  // window.location.href = '/login'; // Redirect to login page
  return Promise.resolve(); // Ensure the function returns a promise
}

/**
 * Register a new user.
 * @param {Object} registrationData
 * @returns {Promise} Registration response data
 */
export async function register(registrationData) {
  const { data } = await api.post("/account/register", registrationData);
  return data;
}

/**
 * Get the current user's profile.
 * @returns {Promise} User profile
 */
export async function getProfile() {
  const { data } = await api.get("/account/me");
  return data;
}

/**
 * Send email confirmation to a user.
 * @param {Object} emailData - { email }
 * @returns {Promise} Email confirmation response
 */
export async function sendConfirmation(emailData) {
  const { data } = await api.post("/account/send-confirmation", emailData);
  return data;
}

/**
 * Confirm a user's email.
 * @param {string} token
 * @returns {Promise} Confirmation response
 */
export async function confirmEmail(token) {
  const { data } = await api.get(`/account/confirm?token=${token}`);
  return data;
} 
