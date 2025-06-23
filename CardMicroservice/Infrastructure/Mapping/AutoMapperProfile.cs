using AutoMapper;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Infrastructure.Scryfall;

namespace CardMicroservice.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ScryfallCardDto, CardDto>()
                // Convert Cmc from float to int
                .ForMember(dst => dst.Cmc, opt => opt.MapFrom(src => (int)Math.Floor(src.Cmc)))
                // Convert Power and Toughness from string to nullable int
                .ForMember(dst => dst.Power, opt => opt.ConvertUsing(new StringToNullableIntConverter(), src => src.Power))
                .ForMember(dst => dst.Toughness, opt => opt.ConvertUsing(new StringToNullableIntConverter(), src => src.Toughness))
                // Map Keywords to Abilities, default to empty list if null
                .ForMember(dst => dst.Abilities, opt => opt.MapFrom(src => src.Keywords ?? new List<string>()))
                // Map ImageUrisData.Normal to ImageUrl, handle null
                .ForMember(dst => dst.ImageUrl, opt => opt.MapFrom(src => src.ImageUrisData != null ? src.ImageUrisData.Normal : null))                
                // Set IsTapped to false by default
                .ForMember(dst => dst.IsTapped, opt => opt.MapFrom(_ => false));
        }
    }

    // Custom converter for string to nullable int
    public class StringToNullableIntConverter : IValueConverter<string?, int?>
    {
        public int? Convert(string? sourceMember, ResolutionContext context)
        {
            return int.TryParse(sourceMember, out int result) ? result : null;
        }
    }
}