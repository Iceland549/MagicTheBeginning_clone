import React from 'react';
import '../game-styles/Library.css';

export default function Library({ count }) {
  return (
    <div className="library-zone">
      <p>Cartes en bibliothèque : {count}</p>
    </div>
  );
}