using AutoMapper;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Infrastructure.Persistence.Entities;

namespace DeckMicroservice.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Maps Entity → DTO
            CreateMap<Deck, DeckDto>();
            CreateMap<DeckCard, DeckCardDto>();

            // Maps DTO → Entity (useful if you change Repository implementation)
            CreateMap<DeckDto, Deck>();
            CreateMap<DeckCardDto, DeckCard>();

            // Maps CreateDeckRequest → Deck entity
            CreateMap<CreateDeckRequest, Deck>();
        }
    }
}
