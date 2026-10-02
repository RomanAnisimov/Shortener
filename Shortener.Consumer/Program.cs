using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Consumer;
using Shortener.Data;
using Shortener.Repository;
using Shortener.Repository.Caching;
using Shortener.Services;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<ICodeService, CodeService>();
builder.Services.AddScoped<ICodeRepository, CodeRepository>();
builder.Services.AddScoped<IKafkaConsumerService, KafkaConsumerService>();

builder.Services.AddScoped<ILinkCache, RedisLinkCache>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"]!));

var host = builder.Build();
host.Run();