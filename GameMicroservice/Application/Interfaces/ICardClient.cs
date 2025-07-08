using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;
namespace GameMicroservice.Infrastructure
{
    public interface ICardClient
    {
        Task<CardDto?> GetCardByIdAsync(string cardId);
    }
}