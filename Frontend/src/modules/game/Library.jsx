import React from 'react';

export default function Library({ count }) {
  return (
    <div className="library-zone">
      <p>Cartes en bibliothèque : {count}</p>
    </div>
  );
}