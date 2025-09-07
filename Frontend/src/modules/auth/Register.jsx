import React, { useState } from 'react';
import { register } from './authService';

export default function Register() {
  const [email, setEmail] = useState('');
  const [pass, setPass]   = useState('');

  const handle = async () => {
    try {
      await register({ email, password: pass });
      alert('Inscription réussie ! Vous pouvez maintenant vous connecter.');
      window.location.href = '/login';
    } catch (err) {
      console.error('Register error', err);
      alert('Erreur lors de l’inscription');
    }
  };

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-cards.jpg)' }}>
      <div className="container">
        <h1 className="app-title">Magic The Beginning</h1>
        <img src="/assets/magic-register.jpg" alt="Illustration inscription" className="logo" />
        <h2>Inscription</h2>
        <input
          value={email}
          onChange={e => setEmail(e.target.value)}
          placeholder="Email"
        />
        <input
          type="password"
          value={pass}
          onChange={e => setPass(e.target.value)}
          placeholder="Mot de passe"
        />
        <button className="btn" onClick={handle}>S’inscrire</button>

        <p style={{ marginTop: '1rem' }}>
          Déjà un compte ?{' '}
          <a href="/login" style={{ color: '#007bff', textDecoration: 'underline' }}>
            Se connecter
          </a>
        </p>
      </div>
    </div>
  );
}
