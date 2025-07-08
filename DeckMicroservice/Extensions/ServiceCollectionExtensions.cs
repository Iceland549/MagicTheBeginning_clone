using DeckMicroservice.Application.Interfaces;
using DeckMicroservice.Application.UseCases;
using DeckMicroservice.Infrastructure.Config;
using DeckMicroservice.Infrastructure.Mapping;
using DeckMicroservice.Infrastructure.Repositories;
using DeckMicroservice.Infrastructure.Clients;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DeckMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeckMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1) Config binding
            services.Configure<MongoDbConfig>(
              configuration.GetSection("Mongo"));

            // 2) MongoClient singleton
            services.AddSingleton(sp => {
                var cfg = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
                return new MongoClient(cfg.ConnectionString);
            });

            // 3) IMongoDatabase singleton
            services.AddSingleton(sp => {
                var cfg = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
                var client = sp.GetRequiredService<MongoClient>();
                return client.GetDatabase(cfg.DatabaseName);
            });

            services.AddHttpClient<ICardClient, CardHttpClient>();


            // 4) Repository Mongo
            services.AddScoped<IDeckRepository, MongoDeckRepository>();

            // 5) AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // 6) UseCases
            services.AddScoped<CreateDeckUseCase>();
            services.AddScoped<ValidateDeckUseCase>();
            services.AddScoped<GetDecksByOwnerUseCase>();

            return services;
        }
    }
}