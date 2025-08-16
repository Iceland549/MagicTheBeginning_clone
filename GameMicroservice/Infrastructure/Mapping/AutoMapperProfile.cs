using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Infrastructure.Persistence.Entities;

namespace GameMicroservice.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Entity → DTO
            CreateMap<GameSession, GameSessionDto>();
            CreateMap<PlayerState, PlayerStateDto>()
                .ForMember(dest => dest.ManaPool, opt => opt.MapFrom(src => src.ManaPool));

            // DTO → Entity
            CreateMap<GameSessionDto, GameSession>();
            CreateMap<PlayerStateDto, PlayerState>()
                .ForMember(dest => dest.ManaPool, opt => opt.MapFrom(src => src.ManaPool));

            CreateMap<CardInGame, CardInGameDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.TypeLine, opt => opt.MapFrom(src => src.TypeLine))
                .ForMember(dest => dest.ManaCost, opt => opt.MapFrom(src => src.ManaCost))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<CardInGameDto, CardInGame>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.TypeLine, opt => opt.MapFrom(src => src.TypeLine))
                .ForMember(dest => dest.ManaCost, opt => opt.MapFrom(src => src.ManaCost))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}