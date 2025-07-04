using CardMicroservice.Application.Interfaces;
using CardMicroservice.Application.UseCases;
using CardMicroservice.Infrastructure.Config;
using CardMicroservice.Infrastructure.Mapping;
using CardMicroservice.Infrastructure.Persistence.Repositories;
using CardMicroservice.Infrastructure.Scryfall;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CardMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCardMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Config binding
            services.Configure<MongoDbConfig>(
              configuration.GetSection("Mongo"));

            // MongoClient singleton
            services.AddSingleton(sp => {
                var cfg = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
                return new MongoClient(cfg.ConnectionString);
            });

            // IMongoDatabase singleton
            services.AddSingleton(sp => {
                var cfg = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
                var client = sp.GetRequiredService<MongoClient>();
                return client.GetDatabase(cfg.DatabaseName);
            });

            // Repositories & clients
            services.AddScoped<ICardRepository, MongoCardRepository>();
            services.AddHttpClient<IScryfallClient, ScryfallHttpClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.scryfall.com");
                client.DefaultRequestHeaders.Add("User-Agent", "MagicTheBeginning/1.0");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

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