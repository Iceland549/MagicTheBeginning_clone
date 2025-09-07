import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from './authService';

export default function Login() {
  const [email, setEmail] = useState('');
  const [pass, setPass]   = useState('');
  const navigate = useNavigate();

  const handle = async () => {
    try {
      console.log("Tentative de connexion avec :", { email, password: pass });
      await login({ email, password: pass });
      console.log('Connexion réussie, redirection vers /dashboard');
      navigate('/dashboard', { replace: true });
    } catch (error) {
      console.error('Échec de la connexion', error);
      alert('Échec de la connexion');
    }
  };

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-cards.jpg)' }}>
      <div className="container">
        <h1 className="app-title">Magic The Beginning</h1>
        <img src="/assets/logo.jpg" alt="Logo" className="logo"/>
        <h2>Connexion</h2>
        <input value={email} onChange={e => setEmail(e.target.value)} placeholder="Email" />
        <input type="password" value={pass} onChange={e => setPass(e.target.value)} placeholder="Mot de passe" />
        <button className="btn" onClick={handle}>Se connecter</button>

        <p style={{ marginTop: '1rem' }}>
          Pas encore de compte ?{' '}
          <a href="/register" style={{ color: '#007bff', textDecoration: 'underline' }}>
            Créer un compte
          </a>
        </p>
      </div>
    </div>
  );
}