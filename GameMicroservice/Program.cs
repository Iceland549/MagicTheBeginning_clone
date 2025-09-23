using GameMicroservice.Application.Interfaces;
using GameMicroservice.Application.UseCases;
using GameMicroservice.Extensions;
using GameMicroservice.Infrastructure.Mapping;
using GameMicroservice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(p =>
  p.AddPolicy("AllowReactApp", b =>
    b.WithOrigins("http://localhost:3000")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));


// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Secret"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Cross origins (frontend React)
builder.Services.AddCors(opts =>
    opts.AddPolicy("AllowAll", p =>
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowAnyOrigin()
    )
);

// HealthCheck
builder.Services.AddHealthChecks();


// Add services to the container.
builder.Services.AddGameMicroserviceServices(builder.Configuration);

var useLocal = builder.Configuration.GetValue<bool>("UseLocalDb", false);

var mongoConn = useLocal
    ? builder.Configuration["Mongo:ConnectionString_Local"]
    : builder.Configuration["Mongo:ConnectionString"];

var dbName = useLocal
    ? builder.Configuration["Mongo:DatabaseName_Local"]
    : builder.Configuration["Mongo:DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConn));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(dbName));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; 
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); 
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MTB Game API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] then your token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] {}
        }
    });
});
var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
