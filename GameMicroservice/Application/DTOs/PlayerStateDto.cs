using System.Collections.Generic;
using GameMicroservice.Domain;

namespace GameMicroservice.Application.DTOs
{
    public class PlayerStateDto
    {
        public string PlayerId { get; set; } = null!; // Reference to the user

        public int LifeTotal { get; set; } = 20; // Remaining life points


        public Dictionary<Color, int> ManaPool { get; set; } = new()
        {
            { Color.White, 0 }, { Color.Blue, 0 }, { Color.Black, 0 }, { Color.Red, 0 }, { Color.Green, 0 }
        }; // Available mana by color

        public int LandsPlayedThisTurn { get; set; } // Number of lands played this turn

        public bool HasDrawnThisTurn { get; set; } // Whether the player has drawn this turn
    }
}