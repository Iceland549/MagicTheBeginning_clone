using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;

namespace GameMicroservice.Application.Interfaces
{
    public interface ICardClient
    {
        Task<CardDto?> GetCardByIdAsync(string cardId);
    }
}