using AutoMapper;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Infrastructure.Persistence.Entities;
using CardMicroservice.Infrastructure.Scryfall;
using System;

namespace CardMicroservice.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ScryfallCardDto, CardDto>()
                .ForMember(dst => dst.NormalizedName, opt => opt.Ignore())
                .ForMember(dst => dst.ScryfallId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dst => dst.ManaCost, opt => opt.MapFrom(src => src.ManaCost))
                .ForMember(dst => dst.TypeLine, opt => opt.MapFrom(src => src.TypeLine))
                .ForMember(dst => dst.OracleText, opt => opt.MapFrom(src => src.OracleText))
                .ForMember(dst => dst.Power, opt => opt.ConvertUsing(new StringToNullableIntConverter(), src => src.Power))
                .ForMember(dst => dst.Toughness, opt => opt.ConvertUsing(new StringToNullableIntConverter(), src => src.Toughness))
                .ForMember(dst => dst.Keywords, opt => opt.MapFrom(src => src.Keywords ?? new List<string>()))
                .ForMember(dst => dst.ImageUrl, opt => opt.MapFrom(src => src.ImageUrisData != null ? src.ImageUrisData.Normal : null))
                .ForMember(dst => dst.Cmc, opt => opt.MapFrom(src => (int)Math.Floor(src.Cmc)))
                .ForMember(dst => dst.Object, opt => opt.MapFrom(src => src.Object))
                .ForMember(dst => dst.OracleId, opt => opt.MapFrom(src => src.OracleId))
                .ForMember(dst => dst.MultiverseIds, opt => opt.MapFrom(src => src.MultiverseIds))
                .ForMember(dst => dst.MtgoId, opt => opt.MapFrom(src => src.MtgoId))
                .ForMember(dst => dst.MtgoFoilId, opt => opt.MapFrom(src => src.MtgoFoilId))
                .ForMember(dst => dst.TcgplayerId, opt => opt.MapFrom(src => src.TcgplayerId))
                .ForMember(dst => dst.CardmarketId, opt => opt.MapFrom(src => src.CardmarketId))
                .ForMember(dst => dst.Lang, opt => opt.MapFrom(src => src.Lang))
                .ForMember(dst => dst.ReleasedAt, opt => opt.MapFrom(src => src.ReleasedAt))
                .ForMember(dst => dst.Uri, opt => opt.MapFrom(src => src.Uri))
                .ForMember(dst => dst.ScryfallUri, opt => opt.MapFrom(src => src.ScryfallUri))
                .ForMember(dst => dst.Layout, opt => opt.MapFrom(src => src.Layout))
                .ForMember(dst => dst.HighresImage, opt => opt.MapFrom(src => src.HighresImage))
                .ForMember(dst => dst.ImageUris, opt => opt.MapFrom(src => src.ImageUrisData != null
                    ? new Dictionary<string, string>
                    {
                        { "small", src.ImageUrisData.Small },
                        { "normal", src.ImageUrisData.Normal },
                        { "large", src.ImageUrisData.Large },
                        { "png", src.ImageUrisData.Png },
                        { "art_crop", src.ImageUrisData.ArtCrop }
                    } : null))
                .ForMember(dst => dst.Colors, opt => opt.MapFrom(src => src.Colors))
                .ForMember(dst => dst.ColorIdentity, opt => opt.MapFrom(src => src.ColorIdentity))
                .ForMember(dst => dst.Legalities, opt => opt.MapFrom(src => src.Legalities))
                .ForMember(dst => dst.Games, opt => opt.MapFrom(src => src.Games))
                .ForMember(dst => dst.Reserved, opt => opt.MapFrom(src => src.Reserved))
                .ForMember(dst => dst.Foil, opt => opt.MapFrom(src => src.Foil))
                .ForMember(dst => dst.Nonfoil, opt => opt.MapFrom(src => src.Nonfoil))
                .ForMember(dst => dst.Finishes, opt => opt.MapFrom(src => src.Finishes))
                .ForMember(dst => dst.Oversized, opt => opt.MapFrom(src => src.Oversized))
                .ForMember(dst => dst.Promo, opt => opt.MapFrom(src => src.Promo))
                .ForMember(dst => dst.Reprint, opt => opt.MapFrom(src => src.Reprint))
                .ForMember(dst => dst.Variation, opt => opt.MapFrom(src => src.Variation))
                .ForMember(dst => dst.SetId, opt => opt.MapFrom(src => src.SetId))
                .ForMember(dst => dst.Set, opt => opt.MapFrom(src => src.Set))
                .ForMember(dst => dst.SetName, opt => opt.MapFrom(src => src.SetName))
                .ForMember(dst => dst.SetType, opt => opt.MapFrom(src => src.SetType))
                .ForMember(dst => dst.SetUri, opt => opt.MapFrom(src => src.SetUri))
                .ForMember(dst => dst.SetSearchUri, opt => opt.MapFrom(src => src.SetSearchUri))
                .ForMember(dst => dst.ScryfallSetUri, opt => opt.MapFrom(src => src.ScryfallSetUri))
                .ForMember(dst => dst.RulingsUri, opt => opt.MapFrom(src => src.RulingsUri))
                .ForMember(dst => dst.PrintsSearchUri, opt => opt.MapFrom(src => src.PrintsSearchUri))
                .ForMember(dst => dst.CollectorNumber, opt => opt.MapFrom(src => src.CollectorNumber))
                .ForMember(dst => dst.Digital, opt => opt.MapFrom(src => src.Digital))
                .ForMember(dst => dst.Rarity, opt => opt.MapFrom(src => src.Rarity))
                .ForMember(dst => dst.FlavorText, opt => opt.MapFrom(src => src.FlavorText))
                .ForMember(dst => dst.CardBackId, opt => opt.MapFrom(src => src.CardBackId))
                .ForMember(dst => dst.Artist, opt => opt.MapFrom(src => src.Artist))
                .ForMember(dst => dst.ArtistIds, opt => opt.MapFrom(src => src.ArtistIds))
                .ForMember(dst => dst.IllustrationId, opt => opt.MapFrom(src => src.IllustrationId))
                .ForMember(dst => dst.BorderColor, opt => opt.MapFrom(src => src.BorderColor))
                .ForMember(dst => dst.Frame, opt => opt.MapFrom(src => src.Frame))
                .ForMember(dst => dst.SecurityStamp, opt => opt.MapFrom(src => src.SecurityStamp))
                .ForMember(dst => dst.FullArt, opt => opt.MapFrom(src => src.FullArt))
                .ForMember(dst => dst.Textless, opt => opt.MapFrom(src => src.Textless))
                .ForMember(dst => dst.Booster, opt => opt.MapFrom(src => src.Booster))
                .ForMember(dst => dst.StorySpotlight, opt => opt.MapFrom(src => src.StorySpotlight))
                .ForMember(dst => dst.EdhrecRank, opt => opt.MapFrom(src => src.EdhrecRank))
                .ForMember(dst => dst.PennyRank, opt => opt.MapFrom(src => src.PennyRank))
                .ForMember(dst => dst.Prices, opt => opt.MapFrom(src => src.Prices))
                .ForMember(dst => dst.RelatedUris, opt => opt.MapFrom(src => src.RelatedUris))
                .ForMember(dst => dst.PurchaseUris, opt => opt.MapFrom(src => src.PurchaseUris))
                .ForMember(dst => dst.Abilities, opt => opt.MapFrom(src => src.Keywords ?? new List<string>()));

            CreateMap<CardEntity, CardDto>()
                .ForMember(dst => dst.Lang, opt => opt.MapFrom(src => src.Language))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<CardDto, CardEntity>()
                .ForMember(dst => dst.Language, opt => opt.MapFrom(src => src.Lang))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id));
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