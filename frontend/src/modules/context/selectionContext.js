import { createContext, useContext, useState } from 'react';

const SelectionContext = createContext();

export function SelectionProvider({ children }) {
  const [selection, setSelection] = useState([]);

  const addCard = (card, quantity = 1) => {
    console.log('Attempting to add card:', JSON.stringify(card, null, 2), 'Quantity:', quantity);
    setSelection(prev => {
      const found = prev.find(c => c.id === card.id);
      if (found) {
        return prev.map(c =>
          c.id === card.id ? { ...c, quantity: (c.quantity || 1) + quantity } : c
        );
      }
      return [...prev, { ...card, quantity }];
    });
  };

  const removeCard = (card) => {
    console.log('Attempting to remove card:', JSON.stringify(card, null, 2));
    setSelection(prev =>
      prev
        .map(c => c.id === card.id ? { ...c, quantity: (c.quantity || 1) - 1 } : c)
        .filter(c => c.quantity > 0)
    );
  };

  const clearSelection = () => {
    console.log('Clearing selection');
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