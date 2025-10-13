using AutoMapper;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using System.Threading.Tasks;

namespace DeckMicroservice.Application.UseCases
{
    public class GetDeckByIdUseCase
    {
        private readonly IDeckRepository _repo;
        private readonly IMapper _mapper;

        public GetDeckByIdUseCase(IDeckRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a single deck by its unique ID, mapping the entity to DTO.
        /// </summary>
        public async Task<DeckDto?> ExecuteAsync(string id)
        {
            var deckEntity = await _repo.GetByIdAsync(id);
            return _mapper.Map<DeckDto>(deckEntity);
        }
    }
}
