import React, { useState } from 'react';
import { register, sendConfirmation } from './authService';

export default function Register() {
  const [email, setEmail] = useState('');
  const [pass, setPass]   = useState('');

  const handle = async () => {
    try {
      await register({ email, password: pass });
      await sendConfirmation(email);
      alert('Inscription OK, email de confirmation envoyé');
      window.location.href = '/login';
    } catch {
      alert('Erreur lors de l’inscription');
    }
  };

  return (
    <div className="section" style={{ backgroundImage: 'url(/assets/bg-cards.jpg)' }}>
      <div className="container">
        <h1 className="app-title">Magic The Beginning</h1>
        <h2>Inscription</h2>
        <input value={email} onChange={e => setEmail(e.target.value)} placeholder="Email" />
        <input type="password" value={pass} onChange={e => setPass(e.target.value)} placeholder="Mot de passe" />
        <button className="btn" onClick={handle}>S’inscrire</button>
      </div>
    </div>
  );
}