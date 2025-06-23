using CardMicroservice.Application.Interfaces;
using CardMicroservice.Application.UseCases;
using CardMicroservice.Infrastructure.Config;
using CardMicroservice.Infrastructure.Mapping;
using CardMicroservice.Infrastructure.Persistence.Repositories;
using CardMicroservice.Infrastructure.Scryfall;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.DependencyInjection;

namespace CardMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCardMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Config Mongo
            services.Configure<MongoDbConfig>(configuration.GetSection("MongoDb"));

            // Repositories & clients
            services.AddScoped<ICardRepository, MongoCardRepository>();
            services.AddHttpClient<IScryfallClient, ScryfallHttpClient>();

            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // UseCases
            services.AddScoped<GetAllCardsUseCase>();
            services.AddScoped<GetCardByNameUseCase>();
            services.AddScoped<ImportCardUseCase>();

            return services;
        }
    }
}