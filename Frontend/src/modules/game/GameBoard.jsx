import React, { useState, useEffect, useCallback } from 'react';
import { startGame, getGameState, playCard, declareAttackers, declareBlockers, resolveCombat } from './gameService';
import { getAllDecks } from '../decks/deckService';
import { getAllCards } from '../cards/cardService'; 
import api from '../../services/api';
import { useNavigate } from 'react-router-dom';
import '../game-styles/GameBoard.css';
import Battlefield from './Battlefield';
import PlayerZone from './PlayerZone';
import GameActions from './GameActions';

export default function GameBoard(showControls, ...props) {
  const [gameId, setGameId] = useState('');
  const [state, setState] = useState(null);
  const [decks, setDecks] = useState([]);
  const [deckP1, setDeckP1] = useState(null);
  const [deckP2, setDeckP2] = useState(null);
  const [loading, setLoading] = useState(false);
  const [cardDetails, setCardDetails] = useState({}); 
  const [showWizard, setShowWizard] = useState(false);
  const [combatMode, setCombatMode] = useState(null);
  // const [localTappedAttackers, setlocalTappedAttackers] = useState([]);
  // const [localSelectedBlockers, setlocalSelectedBlockers] = useState({});
  const [combatError, setCombatError] = useState(null);
  const navigate = useNavigate();
  const playerId = localStorage.getItem('userId'); 
  const resetCombatState = useCallback(() => {
  setCombatMode(null);
  setLocalTappedAttackers([]);
  setLocalSelectedBlockers({});
  setCombatError(null);
  }, []);
  const [showCombatBanner, setShowCombatBanner] = useState(false);
  const [combatPhaseMessage, setCombatPhaseMessage] = useState('');
  const [showDamageBanner, setShowDamageBanner] = useState(false);
  const [damagePhaseMessage, setDamagePhaseMessage] = useState('');

  const [localTappedAttackers, setLocalTappedAttackers] = useState([]); 
  const [localSelectedBlockers, setLocalSelectedBlockers] = useState({});

  

  useEffect(() => {
    if (!playerId) {
      console.error('No playerId found in localStorage');
      navigate('/login');
      return;
    }
    console.log('playerId:', playerId);
    // Récupérer les decks
    getAllDecks()
      .then(response => setDecks(response.data || []))
      .catch(() => setDecks([]));

    // Récupérer toutes les cartes pour les détails
    getAllCards()
      .then(response => {
        console.log('Cards fetched in GameBoard:', JSON.stringify(response, null, 2));
        const cardMap = response.reduce((map, card) => {
          map[card.id] = card; 
          return map;
        }, {});
        setCardDetails(cardMap);
      })
      .catch(error => {
        console.error('Erreur lors de getAllCards:', error);
        setCardDetails({});
      });
  }, [playerId, navigate]);

  const isAITurn = state && state.activePlayerId === state.playerTwoId;

  const refresh = useCallback(async () => {
    if (!gameId) return;
    setLoading(true);
    try {
      const response = await getGameState(gameId);
      console.log('Game state:', JSON.stringify(response, null, 2));
      if (response?.zones) {
        console.log('Zones reçues du backend:', Object.keys(response.zones));
      } else {
        console.warn('Aucune zone reçue dans le game state');
      }
      setState({...response});
    } catch (error) {
      alert('Erreur lors du rafraîchissement de létat du jeu');
    }
    setLoading(false);
  }, [gameId]);

  const init = async () => {
    if (!deckP1 || !deckP2) {
      alert('Sélectionne un deck pour chaque joueur');
      return;
    }
    setLoading(true);
    try {
      const gameData = {
        playerOneId: playerId,
        playerTwoId: 'AI',
        deckIdP1: deckP1.id,
        deckIdP2: deckP2.id,
      };
      const response = await startGame(gameData);
      setGameId(response.id);
      setState(response);
    } catch (error) {
      alert(`Erreur lors du démarrage de la partie : ${error.response?.data?.error || error.message}`);
    }
    setLoading(false);
  };

  const onPlay = async (cardId, actionType = 'PlayCard') => {
      console.log('=== ON PLAY ===');
      console.log('Current Phase:', state?.currentPhase);
      console.log('Action Type:', actionType);
      console.log('Card Name:', cardId);
    if (!gameId || !playerId) {
      console.error(`Invalid parameters: gameId=${gameId}, playerId=${playerId}`);
      alert('Erreur : paramètres manquants.');
      return;
    }

    if (['PlayCard', 'PlayLand'].includes(actionType) && !cardId) {
      console.error(`Invalid cardId for actionType=${actionType}`);
      alert('Erreur : aucune carte sélectionnée.');
      return;
    }

    setLoading(true);
    try {
      const playData = { PlayerId: playerId, CardId: cardId, Type: actionType };
      console.log('Sending playData:', JSON.stringify(playData, null, 2));
      await playCard(gameId, playData);
      await refresh();
    } catch (error) {
      console.error('Erreur lors de laction:', error);
      alert('Erreur lors de laction : ' + (error.message || ''));
    }
    setLoading(false);
  };

  useEffect(() => {
    if (state && state.activePlayerId === state.playerTwoId) {
      setLoading(true);
      api.post(`/games/${gameId}/ai-turn`)
        .then(() => refresh())
        .catch(() => setLoading(false));
    }
      // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state?.activePlayerId, gameId, state?.playerTwoId, refresh]);

  useEffect(() => { 
    if (state && state.activePlayerId === playerId && state.currentPhase === "Draw")  
      {
        setShowWizard(true); 
        setTimeout(() => setShowWizard(false), 4000);  
      }  
      // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state?.activePlayerId, state?.currentPhase]);

  useEffect(() => {
    if (state?.currentPhase !== 'Combat') {
      resetCombatState();
    }
  }, [state?.currentPhase, resetCombatState]);
 
  useEffect(() => {
  if (state?.currentPhase === "Combat") {
    setCombatPhaseMessage("⚔️ Phase de combat !");
    setShowCombatBanner(true);
    setTimeout(() => setShowCombatBanner(false), 4000); 
    }
  }, [state?.currentPhase]);

  useEffect(() => {
  if (state?.currentPhase === "PreEnd") {
    setDamagePhaseMessage("💥 Dégâts résolus !");
    setShowDamageBanner(true);
    setTimeout(() => setShowDamageBanner(false), 4000);
    }
  }, [state?.currentPhase]);


  // useEffect(() => {
  //   if(state && state.currentPhase === "End") {
  //     alert(`Fin de Partie : le joueur ${state.players.find(p => p.playerId === state.activePlayerId)?.name || "inconnu"} a gagné. Bravo !!!`);
  //   }
  // }, [state]);


  const availableDecks = decks.filter(
    d => (!deckP1 || d.id !== deckP1.id) && (!deckP2 || d.id !== deckP2.id)
  );

  const buildActions = () => {
    if (!state || state.activePlayerId !== playerId) return [];

    const normalizeCard = (card) => ({
      ...card,
      cardId: card.cardId || card.CardId,
      name: card.name || card.Name,
    });

    console.log('=== BUILD ACTIONS ===');
    console.log('Current Phase:', state.currentPhase);
    console.log('Active Player:', state.activePlayerId);
    console.log('Player ID:', playerId);

    const hand = (state.zones[`${playerId}_hand`] || []).map(normalizeCard);
    const landCards = hand.filter(c => c.typeLine?.includes('Land'));
    const spellCards = hand.filter(c => !c.typeLine?.includes('Land'));

    const actions = [];

    switch (state.currentPhase) {
      case 'Draw':
        console.log('-> Adding Draw phase actions');
          actions.push({ label: 'Continuer', type: 'Draw' });

        break; 

      case 'Main':
        console.log('-> Adding Main phase actions');
        landCards.forEach(card => {
          actions.push({
            label: `Jouer Terrain: ${card.name}`,
            type: 'PlayLand',
            cardId: card.cardId 
          });
        });

        spellCards.forEach(card => {
          actions.push({
            label: `Jouer Sort: ${card.name}`,
            type: 'PlayCard',
            cardId: card.cardId 
          });
        });

        actions.push({ label: 'Passer à la phase de combat', type: 'PassToCombat' });
        actions.push({ label: 'Finir le tour', type: 'EndTurn' });
        break;

      case 'Combat':
        console.log('-> Adding Combat actions');


        if (state.activePlayerId === playerId) {
          actions.push({ label: 'Déclarer les attaquants', type: 'DeclareAttackers' });
        }

        actions.push({ label: 'Déclarer les bloqueurs', type: 'DeclareBlockers' });

        actions.push({ label: 'Fin de combat', type: 'ResolveCombat' });

        break;


      case 'End':
        console.log('-> Adding End phase actions');
        actions.push({ label: 'Finir le tour', type: 'EndTurn' });
        break;

      default:
        console.warn('Unknown phase:', state.currentPhase);
        break;
    }

    console.log('Actions generated:', actions);
    return actions;
  };

// ============================================
// HANDLERS
// ============================================

  // --- COMBAT PHASE ACTIONS ---

/**
 * Sélectionne/désélectionne une créature attaquante
 */

  // ✅ Nouveau handleTapCreature — tap local uniquement
  const handleTapCreature = (cardId, isTappedNow) => {
    if (state?.currentPhase !== 'Combat') {
      console.warn('⚠️ Tentative de tap de créature hors phase Combat');
      return;
    }

    console.log('[GameBoard] Tap local (Combat phase)', { cardId, isTappedNow });

    setLocalTappedAttackers(prev => {
      if (isTappedNow) {
        // ajoute la créature si elle n’y est pas déjà
        return [...new Set([...prev, cardId])];
      } else {
        // détap si elle était déjà sélectionnée
        return prev.filter(id => id !== cardId);
      }
    });
  };

  /**
   * Assigne un bloqueur à un attaquant
   */
  const handleSelectBlocker = (attackerId, blockerId, isSelected) => {
    setLocalSelectedBlockers(prev => {
      const current = prev[attackerId] || [];
      return {
        ...prev,
        [attackerId]: isSelected
          ? [...new Set([...current, blockerId])]
          : current.filter(id => id !== blockerId),
      };
    });
  };


  /**
   * Déclare les créatures sélectionnées comme attaquantes
   */
  const handleDeclareAttackers = async () => {
    console.log('[Combat] === handleDeclareAttackers START ===');
    console.log('[Combat] GameId:', gameId);
    console.log('[Combat] PlayerId:', playerId);
    console.log('[Combat] Selected attackers:', localTappedAttackers);

    // Validation côté UI
    if (!gameId || !playerId) {
      const error = 'GameId ou PlayerId manquant';
      console.error('[Combat]', error);
      setCombatError(error);
      return;
    }

    if (!localTappedAttackers || localTappedAttackers.length === 0) {
      const error = 'Sélectionnez au moins une créature pour attaquer';
      console.warn('[Combat]', error);
      setCombatError(error);
      return;
    }

    try {
      setCombatError(null);

      const updatedState = await declareAttackers(gameId, playerId, localTappedAttackers);
      
      if (!updatedState) {
        throw new Error('Aucune réponse du serveur');
      }

      console.log('[Combat] ✅ Attaquants déclarés avec succès');
      console.log('[Combat] État mis à jour:', updatedState);

      // Met à jour l'état du jeu
      setState(updatedState);

      // ✅ Vérifie si on doit auto-résoudre le combat
      if (updatedState?.currentPhase === 'Combat') {
        const defender = updatedState.players.find(p => p.playerId !== playerId);
        const defenderBattlefield = updatedState.zones?.[`${defender.playerId}_battlefield`] || [];
        const defenderIsAI = defender.playerId === 'AI' || updatedState.isPlayerTwoAI;

        if (defenderIsAI && defenderBattlefield.length === 0) {
          console.log('[AutoResolve] Défenseur IA sans créatures : déclenchement automatique de la résolution du combat...');
          await resolveCombat(gameId, playerId);
          await refresh();
        }
      }

      // Passe en mode blocage
      setCombatMode('block');

      console.log('[Combat] === handleDeclareAttackers END (SUCCESS) ===');

    } catch (error) {
      console.error('[Combat] ❌ Erreur lors de la déclaration des attaquants:', error);
      
      // Message utilisateur
      const errorMsg = error.message || error.error || 'Erreur lors de la déclaration des attaquants';
      setCombatError(errorMsg);

      // Réinitialise le mode combat si erreur critique
      if (error.status === 401 || error.status === 404) {
        setCombatMode(null);
        setLocalTappedAttackers([]);
      }

      console.log('[Combat] === handleDeclareAttackers END (ERROR) ===');
    }
  };

  /**
  * Déclare les bloqueurs et résout le combat
  */
  const handleDeclareBlockers = async () => {
    console.log('[Combat] === handleDeclareBlockers START ===');
    console.log('[Combat] GameId:', gameId);
    console.log('[Combat] PlayerId:', playerId);
    console.log('[Combat] Selected blockers:', localSelectedBlockers);

    if (!gameId || !playerId) {
      const error = 'GameId ou PlayerId manquant';
      console.error('[Combat]', error);
      setCombatError(error);
      return;
    }

    if (!localSelectedBlockers || typeof localSelectedBlockers !== 'object') {
      const error = 'Format de bloqueurs invalide';
      console.error('[Combat]', error);
      setCombatError(error);
      return;
    }

    try {
      setCombatError(null);

      // 1️⃣ Déclaration des bloqueurs
      console.log('[Combat] Étape 1/3 : Déclaration des bloqueurs...');
      const payloadBlockers =
        localSelectedBlockers && Object.keys(localSelectedBlockers).length > 0
          ? localSelectedBlockers
          : {};

      const stateAfterBlockers = await declareBlockers(gameId, playerId, payloadBlockers);

      if (!stateAfterBlockers) throw new Error('Aucune réponse après déclaration des bloqueurs');

      console.log('[Combat] ✅ Bloqueurs déclarés');
      setState(stateAfterBlockers);

      // 2️⃣ Attente visuelle — ton image "damage-phase-banner" arrive ensuite
      console.log('[Combat] Étape 2/3 : Appel ResolveCombat (IA reprend la main)');
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // ⚔️  Envoi du signal de résolution de combat
      const stateAfterResolve = await resolveCombat(gameId, playerId);
      if (!stateAfterResolve) throw new Error('Aucune réponse après résolution du combat');

      console.log('[Combat] ✅ Combat résolu');
      setState(stateAfterResolve);

      // 3️⃣ Rafraîchir l’état complet (pour déclencher la phase de dégâts & la bannière)
      console.log('[Combat] Étape 3/3 : Refresh de l’état...');
      await refresh();

      // 4️⃣ Réinitialisation de l’UI combat
      setCombatMode(null);
      setLocalTappedAttackers([]);
      setLocalSelectedBlockers({});

      console.log('[Combat] === handleDeclareBlockers END (SUCCESS) ===');
    } catch (error) {
      console.error('[Combat] ❌ Erreur handleDeclareBlockers:', error);
      setCombatError(error.message || 'Erreur pendant la phase de blocage/résolution');
      try {
        await refresh();
      } catch {
        console.warn('Refresh échoué après erreur combat.');
      }
    }
  };


  const handleTapLand = async (cardId, ownerIdFromUI) => {
      console.log('[GameBoard] handleTapLand called', { gameId, cardId, ownerIdFromUI, playerId }); 
    try {
      const playData = { PlayerId: playerId, CardId: cardId, Type: 'TapLand' };
      console.log('[GameBoard] Sending TapLand', playData);
      await playCard(gameId, playData);
      await refresh();
    } catch (error) {
      console.error('Erreur TapLand:', error);
    }
  };


  
  const handleAction = async (action) => {
    try {
      console.log("=== HANDLE ACTION ===");
      console.log("Action received:", action);

      if (!gameId || !playerId) {
        console.error("handleAction: gameId or playerId missing");
        alert("Erreur : paramètres manquants");
        return;
      }

      // Cas 1: Actions liées aux cartes (PlayCard, PlayLand)
      if (["PlayCard", "PlayLand"].includes(action.type)) {
        if (!action.cardId) {
          alert(`Erreur : aucune carte sélectionnée pour ${action.type}`);
          return;
        }
        await onPlay(action.cardId, action.type);
      } 
      // Cas 2: Actions simples (Draw, EndTurn, PassToCombat, PreEnd)
      else {
        const playData = { PlayerId: playerId, CardId: null, Type: action.type };
        console.log("Sending non-card action:", JSON.stringify(playData, null, 2));

      const updatedState = await playCard(gameId, playData);
      if (updatedState?.zones) {
        console.log("🟢 Nouveau gameState reçu directement:", updatedState);
        setState(updatedState);
      } else {
        console.warn("⚠️ Aucun gameState dans la réponse, fallback vers refresh()");
        await refresh();
      }

      }

    } catch (error) {
      console.error("Erreur lors de handleAction:", error);
      alert("Erreur lors de l'action : " + (error.message || "inconnue"));
    }
  };

  // ============================================
  // Enrichir les zones avec les détails des cartes
  // ============================================
  const enrichedZones = state
    ? Object.keys(state.zones).reduce((acc, zoneKey) => {
        acc[zoneKey] = state.zones[zoneKey].map(raw => {
          const ownerIdFromZone = zoneKey.split('_')[0]; 
          return {
            ...raw,
            cardId: raw.CardId || raw.cardId || raw.instanceId || null,
            name: raw.cardName || raw.name || raw.Name || '',
            typeLine: raw.typeLine || raw.TypeLine || '',
            imageUrl: raw.imageUrl || cardDetails[raw.cardId || raw.CardId || raw.cardName]?.imageUrl || 'https://via.placeholder.com/100',
            ownerId: raw.ownerId || raw.owner || raw.controllerId || ownerIdFromZone || null,
            isTapped: Boolean(raw.isTapped),
            CanBeTapped: Boolean(raw.CanBeTapped),
            hasSummoningSickness: Boolean(raw.hasSummoningSickness),
            ...raw
          };
        });
        return acc;
      }, {})
    : {};

  if (state) {
    console.log("♻️ Re-render GameBoard — Zones enrichies:", Object.keys(enrichedZones));
    console.log("🖐 Main joueur :", enrichedZones[`${playerId}_hand`]);
  }

    return (
    <div
      className="game-table-bg"
      style={{
        backgroundImage:
          "url('/blood-artist.jpg'), linear-gradient(135deg,#222 0%,#444 100%)",
      }}
    >
      <div className="game-table-container">
        <button className="btn-back" onClick={() => navigate('/dashboard')}>
          ← Retour au Dashboard
        </button>
        <h2>Plateau de jeu</h2>

        {/* ----------- Sélection des decks ----------- */}
        {!state && (
          <>
            <div className="available-decks-zone">
              <h3>Decks disponibles</h3>
              <ul className="deck-list">
                {availableDecks.map((deck) => (
                  <li key={deck.id} className="deck-item">
                    <span>
                      {deck.name}{' '}
                      <span className="deck-count">
                        {deck.cards.reduce((sum, c) => sum + c.quantity, 0)} cartes
                      </span>
                    </span>
                    <span>
                      <button className="btn-assign" onClick={() => setDeckP1(deck)}>
                        + Joueur 1
                      </button>
                      <button className="btn-assign" onClick={() => setDeckP2(deck)}>
                        + IA
                      </button>
                    </span>
                  </li>
                ))}
              </ul>
            </div>

            <div className="deck-selection-area">
              <div className="deck-selector">
                <h3>Deck Joueur 1 (toi)</h3>
                {deckP1 ? (
                  <div className="deck-item selected">
                    {deckP1.name}{' '}
                    <span className="deck-count">
                      {deckP1.cards.reduce((sum, c) => sum + c.quantity, 0)} cartes
                    </span>
                    <button className="btn-remove" onClick={() => setDeckP1(null)}>
                      -
                    </button>
                  </div>
                ) : (
                  <div className="deck-item empty">Aucun deck sélectionné</div>
                )}
              </div>

              <div className="deck-selector">
                <h3>Deck Joueur 2 (IA)</h3>
                {deckP2 ? (
                  <div className="deck-item selected">
                    {deckP2.name}{' '}
                    <span className="deck-count">
                      {deckP2.cards.reduce((sum, c) => sum + c.quantity, 0)} cartes
                    </span>
                    <button className="btn-remove" onClick={() => setDeckP2(null)}>
                      -
                    </button>
                  </div>
                ) : (
                  <div className="deck-item empty">Aucun deck sélectionné</div>
                )}
              </div>
            </div>

            <button
              className="btn"
              onClick={init}
              disabled={!deckP1 || !deckP2 || loading}
            >
              Démarrer la partie
            </button>
          </>
        )}

        {/* ----------- Plateau de jeu ----------- */}
        {state && (
          <div className="magic-board">
            {/* ---------- TOP BAR : actions + statut ---------- */}
            <div className="top-bar">
              <GameActions actions={buildActions()} onAction={handleAction} />
              {showControls && (
                <div className="actions-zone">
                  <GameActions {...props.actionsProps} />
                </div>
              )}
              <div className="game-status">
                <p>
                  Joueur actif :{' '}
                  {state.activePlayerId === playerId ? 'Toi' : 'IA'}
                </p>
                <p>Phase : {state.currentPhase}</p>
                <p>
                  PV Toi :{' '}
                  {state.players?.find((p) => p.playerId === playerId)?.lifeTotal ??
                    20}
                </p>
                <p>
                  PV IA :{' '}
                  {state.players?.find((p) => p.playerId === state.playerTwoId)
                    ?.lifeTotal ?? 20}
                </p>
              </div>
            </div>

            {/* ---------- MAIN AREA ---------- */}
            <div className="main-area">
              <div className="player-zone-col left">
                <PlayerZone
                  playerId={state.playerTwoId}
                  zones={enrichedZones}
                  onPlayCard={onPlay}
                  isPlayable={false}
                  isHuman={false}
                />
              </div>

              <div className="battlefield-areas">
                <Battlefield
                  cards={enrichedZones[`${state.playerTwoId}_battlefield`] || []}
                  label={`Champ de bataille : ${state.playerTwoId}`}
                  playerId={state.playerTwoId}
                  currentPlayerId={playerId}
                  currentPhase={state?.currentPhase}
                  combatMode={combatMode}
                  selectedAttackers={localTappedAttackers}
                  selectedBlockers={localSelectedBlockers}
                  onSelectAttacker={handleTapCreature}
                  onSelectBlocker={handleSelectBlocker}
                />

                <Battlefield
                  cards={enrichedZones[`${playerId}_battlefield`] || []}
                  label={`Champ de bataille : Toi`}
                  playerId={playerId}
                  currentPlayerId={playerId}
                  currentPhase={state?.currentPhase}
                  combatMode={combatMode}
                  onTap={handleTapLand}
                  onTapCreature={handleTapCreature}
                  selectedAttackers={localTappedAttackers}
                  selectedBlockers={localSelectedBlockers}
                  onSelectAttacker={handleTapCreature}
                  onSelectBlocker={handleSelectBlocker}
                />
              </div>

              <div className="player-zone-col right">
                <PlayerZone
                  playerId={playerId}
                  zones={enrichedZones}
                  onPlayCard={onPlay}
                  isPlayable={
                    state.activePlayerId === playerId &&
                    state.currentPhase === 'Main'
                  }
                  isHuman={true}
                />
              </div>
            </div> {/* end main-area */}
          </div>
        )}
        {/* ----------- Bannières / Phases ----------- */}
        {isAITurn && (
          <div className="ai-thinking-overlay">
            <img
              src="/assets/saruman-2.jpeg"
              alt="AI Thinking"
              className="ai-thinking-image"
            />
          </div>
        )}
        {showWizard && (
          <div className="player-turn-banner">
            <img
              src="/assets/gandalf.jpeg"
              alt="À toi de jouer !"
              className="player-turn-image"
            />
            <div className="player-turn-text">À toi de jouer !...</div>
          </div>
        )}

        {showCombatBanner && (
          <div className="combat-phase-banner">
            <img
              src="/assets/arena-magic.jpg"
              alt="Phase de combat"
              className="combat-phase-image"
            />
            <div className="combat-phase-text">{combatPhaseMessage}</div>
          </div>
        )}

        {showDamageBanner && (
          <div className="damage-phase-banner">
            <img
              src="/assets/damage.jpeg"
              alt="Résolution des dégâts"
              className="damage-phase-image"
            />
            <div className="damage-phase-text">{damagePhaseMessage}</div>
          </div>
        )}

        {state?.currentPhase === 'Combat' &&
          state.activePlayerId === playerId && (
            <div className="combat-actions">
              {combatError && <div className="combat-error">⚠️ {combatError}</div>}

              {!combatMode && (
                <button
                  className="btn btn-combat"
                  onClick={() => setCombatMode('attack')}
                >
                  Déclarer les attaquants
                </button>
              )}

              {combatMode === 'attack' && (
                <div className="combat-attack-buttons">
                  <button
                    className="btn btn-confirm"
                    onClick={handleDeclareAttackers}
                    disabled={localTappedAttackers.length === 0}
                  >
                    Confirmer les attaquants {localTappedAttackers.length}
                  </button>
                  <button className="btn btn-cancel" onClick={resetCombatState}>
                    Annuler
                  </button>
                </div>
              )}

              {combatMode === 'block' && (
                <div className="combat-block-buttons">
                  <p className="combat-info">
                    Phase de blocage – Sélectionne tes bloqueurs ou passe
                  </p>
                  <button
                    className="btn btn-block"
                    onClick={handleDeclareBlockers}
                  >
                    Déclarer les bloqueurs{' '}
                    {Object.keys(localSelectedBlockers).length}
                  </button>
                  <button className="btn btn-skip" onClick={handleDeclareBlockers}>
                    Passer (pas de blocage)
                  </button>
                </div>
              )}
            </div>
          )}
      </div>
    </div>
  );
}