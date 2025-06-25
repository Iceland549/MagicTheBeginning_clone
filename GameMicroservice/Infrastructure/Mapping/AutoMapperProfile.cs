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
        }
    }
}