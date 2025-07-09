import React from 'react';

export default function GameActions({ actions, onAction }) {
  if (!actions || !actions.length) return null;

  return (
    <div className="actions-zone">
      {actions.map((action, i) => (
        <button
          key={i}
          className="btn"
          onClick={() => onAction(action)}
          disabled={action.disabled}
        >
          {action.label}
        </button>
      ))}
    </div>
  );
}