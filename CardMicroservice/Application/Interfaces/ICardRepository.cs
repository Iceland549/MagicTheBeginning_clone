using CardMicroservice.Application.DTOs;

namespace CardMicroservice.Application.Interfaces
{
    public interface ICardRepository
    {
        Task<List<CardDto>> GetAllAsync();
        Task<CardDto?> GetByNameAsync(string name);
        Task AddAsync(CardDto card);
        Task<bool> DeleteByNameAsync(string name);
    }
}