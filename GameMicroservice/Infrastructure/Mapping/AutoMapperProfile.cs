using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Infrastructure.Persistence.Entities;

namespace GameMicroservice.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Entity -> DTO
            CreateMap<GameSession, GameSessionDto>();

            CreateMap<PlayerState, PlayerStateDto>()
                .ForMember(dest => dest.ManaPool, opt => opt.MapFrom(src => src.ManaPool));

            CreateMap<CardInGame, CardInGameDto>()
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.CardName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TypeLine, opt => opt.MapFrom(src => src.TypeLine))
                .ForMember(dest => dest.ManaCost, opt => opt.MapFrom(src => src.ManaCost))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.Power))
                .ForMember(dest => dest.Toughness, opt => opt.MapFrom(src => src.Toughness))
                .ForMember(dest => dest.IsTapped, opt => opt.MapFrom(src => src.IsTapped))
                .ForMember(d => d.IsTapped, o => o.MapFrom(s => s.IsTapped))
                .ForMember(dest => dest.HasSummoningSickness, opt => opt.MapFrom(src => src.HasSummoningSickness))
                .ReverseMap();


            // Map GameSession -> ActionResultDto (used by PlayerPlayTurnUseCase)
            CreateMap<GameSession, ActionResultDto>()
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Message, opt => opt.Ignore())
                .ForMember(dest => dest.GameState, opt => opt.MapFrom(src => src)) // relies on GameSession -> GameSessionDto
                .ForMember(dest => dest.EndGame, opt => opt.Ignore());

            // DTO -> Entity
            CreateMap<GameSessionDto, GameSession>();
            CreateMap<PlayerStateDto, PlayerState>()
                .ForMember(dest => dest.ManaPool, opt => opt.MapFrom(src => src.ManaPool));

            CreateMap<CardInGameDto, CardInGame>()
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CardName))
                .ForMember(dest => dest.TypeLine, opt => opt.MapFrom(src => src.TypeLine))
                .ForMember(dest => dest.ManaCost, opt => opt.MapFrom(src => src.ManaCost))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.Power))
                .ForMember(dest => dest.Toughness, opt => opt.MapFrom(src => src.Toughness))
                .ForMember(dest => dest.IsTapped, opt => opt.MapFrom(src => src.IsTapped))
                .ForMember(dest => dest.HasSummoningSickness, opt => opt.MapFrom(src => src.HasSummoningSickness));
        }
    }
}