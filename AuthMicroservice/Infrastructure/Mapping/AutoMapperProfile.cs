using AutoMapper;
using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mappe User -> ProfileDto automatiquement
            CreateMap<User, ProfileDto>();
            CreateMap<EmailToken, EmailTokenDto>();
            CreateMap<EmailTokenDto, EmailToken>();
        }
    }
}