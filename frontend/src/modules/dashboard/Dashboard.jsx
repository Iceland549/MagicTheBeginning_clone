import { useNavigate } from 'react-router-dom';
import { logout } from '../auth/authService';
import './Dashboard.css';

export default function Dashboard() {
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="dashboard-bg" style={{ backgroundImage: 'url(/assets/endless-ranks-of-the-dead.jpg)' }}>
      <h1>Magic The Beginning</h1>
      <div className="dashboard-options">
        <button onClick={() => navigate('/cards')}>
          <img src="/assets/bg-cards.jpg" alt="Cards" />
          <span>Search Cards</span>
        </button>
        <button onClick={() => navigate('/decks')}>
          <img src="/assets/bg-deck.jpg" alt="Decks" />
          <span>Manage Decks</span>
        </button>
        <button onClick={() => navigate('/game')}>
          <img src="/assets/bg-game.jpg" alt="Game" />
          <span>Play Game</span>
        </button>
        <button onClick={handleLogout}>
          <img src="/assets/dark-ritual.jpg" alt="Deconnexion" />
          <span>Deconnexion</span>
        </button>
      </div>
    </div>
  );
}
