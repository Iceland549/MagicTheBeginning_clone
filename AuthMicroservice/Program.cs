using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Application.UseCases;
using AuthMicroservice.Infrastructure.Config;
using AuthMicroservice.Infrastructure.Mapping;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using AuthMicroservice.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthMicroservice.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HealthCheck
builder.Services.AddHealthChecks();

// DbContext SQL Server with retry
builder.Services.AddDbContext<AuthDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Configurations
builder.Services.AddAuthMicroserviceServices(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentification JWT
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig.Secret))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Migration and Seed Admin with retry
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var retry = 0;
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    while (retry < maxRetries)
    {
        try
        {
            var context = services.GetRequiredService<AuthDbContext>();
            await context.Database.MigrateAsync();
            var seeder = services.GetRequiredService<SeedAdminUseCase>();
            await seeder.ExecuteAsync(
                builder.Configuration["Admin:Email"]!,
                builder.Configuration["Admin:Password"]!
            );
            break;
        }
        catch (Exception ex)
        {
            retry++;
            Console.WriteLine($"Attempt {retry}/{maxRetries} failed: {ex.Message}");
            await Task.Delay(delay);
        }
    }
    if (retry == maxRetries)
    {
        Console.WriteLine("Failed to seed the database after multiple retries.");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Show detailed errors in development
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error"); // Route for global error handling
}


app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
