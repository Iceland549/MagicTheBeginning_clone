using AutoMapper;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Infrastructure.Scryfall;
using CardMicroservice.Utils;

namespace CardMicroservice.Application.UseCases
{
    /// <summary>
    /// Récupère une carte depuis l’API Scryfall, la mappe et la sauvegarde dans Mongo.
    /// </summary>
    public class ImportCardUseCase
    {
        private readonly IScryfallClient _client;
        private readonly ICardRepository _repo;
        private readonly IMapper _mapper;

        public ImportCardUseCase(IScryfallClient client, ICardRepository repo, IMapper mapper)
        {
            _client = client;
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CardDto?> ExecuteAsync(string name, string? set = null, string? lang = null, string? collectorNumber = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // 🔹 Récupère la carte depuis Scryfall
            var raw = await _client.FetchByNameAsync(name, set, lang, collectorNumber);
            if (raw == null)
                return null;

            // 🔹 Transforme en CardDto
            var dto = _mapper.Map<CardDto>(raw);
            dto.NormalizedName = NameNormalizer.Normalize(dto.Name);

            // 🔹 Sauvegarde en base Mongo
            await _repo.AddAsync(dto);
            Console.WriteLine($"[ImportCard] ✅ Carte '{dto.Name}' importée et ajoutée en base");
            return dto;
        }
    }
}
