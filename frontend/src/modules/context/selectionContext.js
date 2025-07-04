import { createContext, useContext, useState } from 'react';

const SelectionContext = createContext();

export function SelectionProvider({ children }) {
  const [selection, setSelection] = useState([]);

  const addCard = (card) => {
    console.log('Attempting to add card:', card); // Log pour déboguer
    setSelection(prev => {
      const found = prev.find(c => c.cardName === card.name);
      if (found) {
        return prev.map(c => c.cardName === card.name ? { ...c, quantity: c.quantity + 1 } : c);
      }
      return [...prev, { cardName: card.name, quantity: 1, image_url: card.image_url }];
    });
  };

  const removeCard = (cardName) => {
    setSelection(prev =>
      prev
        .map(c => c.cardName === cardName ? { ...c, quantity: c.quantity - 1 } : c)
        .filter(c => c.quantity > 0) // enlève la carte si quantité arrive à 0
    );
  };

  const clearSelection = () => {
    setSelection([]);
  };

  return (
    <SelectionContext.Provider value={{ selection, addCard, removeCard, clearSelection }}>
      {children}
    </SelectionContext.Provider>
  );
}

export function useSelection() {
  return useContext(SelectionContext);
}