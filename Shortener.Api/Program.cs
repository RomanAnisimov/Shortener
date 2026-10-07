using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Application.Options;
using Shortener.Data;
using Shortener.Infrastructure.Messaging;
using Shortener.Repository;
using Shortener.Repository.Caching;
using Shortener.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppOptions>(
    builder.Configuration.GetSection(AppOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
builder.Services.AddScoped<IKafkaConsumerService, KafkaConsumerService>();

builder.Services.AddScoped<ICodeService, CodeService>();
builder.Services.AddScoped<ICodeRepository, CodeRepository>();

builder.Services.AddScoped<ILinkCache, RedisLinkCache>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"]!));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

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
