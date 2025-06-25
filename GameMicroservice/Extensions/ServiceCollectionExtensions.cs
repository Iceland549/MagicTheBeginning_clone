using GameMicroservice.Application.Interfaces;
using GameMicroservice.Application.UseCases;
using GameMicroservice.Infrastructure;
using GameMicroservice.Infrastructure.AI;
using GameMicroservice.Infrastructure.Config;
using GameMicroservice.Infrastructure.Mapping;
using GameMicroservice.Infrastructure.Persistence;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace GameMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Config MongoDB
            services.Configure<MongoDbConfig>(configuration.GetSection("Mongo"));

            // 2. Repository MongoDB pour les sessions de jeu
            services.AddScoped<IGameSessionRepository, MongoGameSessionRepository>();

            // 3. Clients HTTP pour les cartes et les decks
            services.AddHttpClient<ICardClient, CardHttpClient>();
            services.AddHttpClient<IDeckClient, DeckHttpClient>();

            // 4. Moteur de règles du jeu
            services.AddScoped<IGameRulesEngine, GameRulesEngine>();

            // 5. Moteur d'IA
            services.AddScoped<IAIEngine, RandomAIEngine>();

            // 6. AutoMapper pour les mappings entre entités et DTOs
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // 7. Use Cases
            services.AddScoped<StartGameUseCase>();
            services.AddScoped<PlayCardUseCase>();
            services.AddScoped<GetGameStateUseCase>();
            services.AddScoped<AIPlayTurnUseCase>();

            return services;
        }
    }
}