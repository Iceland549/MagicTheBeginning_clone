using DeckMicroservice.Application.Interfaces;
using DeckMicroservice.Application.UseCases;
using DeckMicroservice.Infrastructure.Config;
using DeckMicroservice.Infrastructure.Mapping;
using DeckMicroservice.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeckMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeckMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Configuration MongoDb
            services.Configure<MongoDbConfig>(configuration.GetSection("MongoDb"));

            // 2. Repository Mongo
            services.AddScoped<IDeckRepository, MongoDeckRepository>();

            // 3. AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // 4. UseCases
            services.AddScoped<CreateDeckUseCase>();
            services.AddScoped<ValidateDeckUseCase>();
            services.AddScoped<GetDecksByOwnerUseCase>();

            return services;
        }
    }
}