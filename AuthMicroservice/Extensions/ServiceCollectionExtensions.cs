using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Application.UseCases;
using AuthMicroservice.Infrastructure.Config;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Repositories;
using AuthMicroservice.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using AuthMicroservice.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace AuthMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurations
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));

            // Repositories
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IRoleRepository, EfRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
            services.AddScoped<IServiceClientRepository, EfServiceClientRepository>();

            // Security services
            services.AddScoped<IJwtService, JwtTokenGenerator>();  // Pure JWT gen/validation
            services.AddScoped<IAuthService, AuthService>();       // Auth flow orchestration

            // UseCases
            services.AddScoped<RegisterUserUseCase>();
            services.AddScoped<LoginUseCase>();
            services.AddScoped<LogoutUseCase>();
            services.AddScoped<RefreshTokenUseCase>();
            services.AddScoped<SeedAdminUseCase>();
            services.AddScoped<SeedServiceClientsUseCase>();
            services.AddScoped<GetProfileUseCase>();
            services.AddScoped<GenerateResetPasswordUseCase>();
            services.AddScoped<ResetPasswordUseCase>();
            services.AddScoped<GenerateServiceTokenUseCase>();

            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            return services;
        }
    
        public static IServiceCollection AddAuthDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }
    }
}