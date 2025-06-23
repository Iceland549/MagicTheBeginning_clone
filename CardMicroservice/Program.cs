using CardMicroservice.Extensions;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Application.UseCases;
using CardMicroservice.Infrastructure.Mapping;
using CardMicroservice.Infrastructure.Persistence;
using CardMicroservice.Infrastructure.Scryfall;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCardMicroserviceServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
