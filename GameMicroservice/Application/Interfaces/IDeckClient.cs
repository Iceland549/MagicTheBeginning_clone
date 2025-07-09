using System.Collections.Generic;
using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;

namespace GameMicroservice.Infrastructure
{
    public interface IDeckClient
    {
        Task<List<DeckDto>> GetDecksByOwnerAsync(string ownerId);
        Task<List<DeckDto>> GetAllDecksAsync();

    }
}