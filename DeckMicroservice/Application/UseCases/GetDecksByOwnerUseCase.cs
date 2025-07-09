using AutoMapper;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;

namespace DeckMicroservice.Application.UseCases
{
    public class GetDecksByOwnerUseCase
    {
        private readonly IDeckRepository _repo;
        private readonly IMapper _mapper;
        public GetDecksByOwnerUseCase(IDeckRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // Retrieves all decks belonging to a specific owner.
        public Task<List<DeckDto>> ExecuteAsync(string ownerId) =>
            _repo.GetByOwnerAsync(ownerId);

        public async Task<List<DeckDto>> ExecuteAllAsync()
        {
            var decks = await _repo.GetAllDecksAsync();
            return decks.Select(d => _mapper.Map<DeckDto>(d)).ToList();
        }

    }
}