import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login       from './modules/auth/Login';
import Register    from './modules/auth/Register';
import CardList    from './modules/cards/CardList';
import DeckBuilder from './modules/decks/DeckBuilder';
import GameBoard   from './modules/game/GameBoard';

function App() {
  const token = localStorage.getItem('accessToken');
  const loggedIn = !!token;

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/cards" replace />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        {loggedIn && (
          <>
            <Route path="/cards" element={<CardList />} />
            <Route path="/decks" element={<DeckBuilder />} />
            <Route path="/game"  element={<GameBoard />} />
          </>
        )}
        <Route path="*" element={<Navigate to={ loggedIn ? "/cards" : "/login" } replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;