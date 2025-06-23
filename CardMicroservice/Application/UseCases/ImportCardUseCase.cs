using AutoMapper;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Infrastructure.Scryfall;

namespace CardMicroservice.Application.UseCases
{
    public class ImportCardUseCase
    {
        private readonly IScryfallClient _client;
        private readonly ICardRepository _repo;
        private readonly IMapper _mapper;

        public ImportCardUseCase(
            IScryfallClient client,
            ICardRepository repo,
            IMapper mapper)
        {
            _client = client;
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CardDto?> ExecuteAsync(string name)
        {
            // Récupère ScryfallCardDto (brut)
            var raw = await _client.FetchByNameAsync(name);
            if (raw == null) return null;

            // Mappe vers notre CardDto
            var dto = _mapper.Map<CardDto>(raw);

            // Stocke en base Mongo
            await _repo.AddAsync(dto);
            return dto;
        }
    }
}