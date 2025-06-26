import React, { useState } from 'react';
import { login } from './authService';

export default function Login() {
  const [email, setEmail] = useState('');
  const [pass, setPass]   = useState('');

  const handle = async () => {
    try {
      await login({ email, password: pass });
      window.location.href = '/cards';
    } catch {
      alert('Échec de la connexion');
    }
  };

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-cards.jpg)' }}>
      <div className="container">
        <img src="/assets/logo.png" alt="Logo" className="logo"/>
        <h2>Connexion</h2>
        <input value={email} onChange={e => setEmail(e.target.value)} placeholder="Email" />
        <input type="password" value={pass} onChange={e => setPass(e.target.value)} placeholder="Mot de passe" />
        <button className="btn" onClick={handle}>Se connecter</button>
      </div>
    </div>
  );
}