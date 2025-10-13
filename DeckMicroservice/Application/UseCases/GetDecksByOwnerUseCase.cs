using AutoMapper;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // Retrieves all decks belonging to a specific owner, mapping entities to DTOs.
        public async Task<List<DeckDto>> ExecuteAsync(string ownerId)
        {
            var deckEntities = await _repo.GetByOwnerAsync(ownerId);
            return _mapper.Map<List<DeckDto>>(deckEntities);
        }

        public async Task<List<DeckDto>> ExecuteAllAsync()
        {
            var deckEntities = await _repo.GetAllDecksAsync();
            return _mapper.Map<List<DeckDto>>(deckEntities);
        }
    }
}
