using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;

namespace CardMicroservice.Application.UseCases
{
    public class GetAllCardsUseCase
    {
        private readonly ICardRepository _repo;
        public GetAllCardsUseCase(ICardRepository repo) => _repo = repo;

        public Task<List<CardDto>> ExecuteAsync() => _repo.GetAllAsync();
    }
}
