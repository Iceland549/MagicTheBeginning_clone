import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import Login from './modules/auth/Login';
import Register from './modules/auth/Register';
import CardList from './modules/cards/CardList';
import DeckBuilder from './modules/decks/DeckBuilder';
import GameBoard from './modules/game/GameBoard';
import Dashboard from './modules/dashboard/Dashboard';
import { SelectionProvider } from './modules/context/selectionContext';

function App() {
  const [loggedIn, setLoggedIn] = useState(!!localStorage.getItem('accessToken'));

  useEffect(() => {
    const handleStorageChange = () => {
      console.log('localStorage changé, mise à jour de loggedIn');
      setLoggedIn(!!localStorage.getItem('accessToken'));
    };

    window.addEventListener('storage', handleStorageChange);
    const interval = setInterval(() => {
      setLoggedIn(!!localStorage.getItem('accessToken'));
    }, 1000);

    return () => {
      window.removeEventListener('storage', handleStorageChange);
      clearInterval(interval);
    };
  }, []);

  console.log('App.jsx - Token:', localStorage.getItem('accessToken'));
  console.log('App.jsx - loggedIn:', loggedIn);
  console.log('App.jsx - URL actuelle:', window.location.pathname);

  return (
    <SelectionProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={loggedIn ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />} />
          <Route path="/login" element={!loggedIn ? <Login /> : <Navigate to="/dashboard" replace />} />
          <Route path="/register" element={!loggedIn ? <Register /> : <Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={loggedIn ? <Dashboard /> : <Navigate to="/login" replace />} />
          <Route path="/cards" element={loggedIn ? <CardList /> : <Navigate to="/login" replace />} />
          <Route path="/decks" element={loggedIn ? <DeckBuilder /> : <Navigate to="/login" replace />} />
          <Route path="/game" element={loggedIn ? <GameBoard /> : <Navigate to="/login" replace />} />
          <Route path="*" element={loggedIn ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </SelectionProvider>
  );
}

export default App;