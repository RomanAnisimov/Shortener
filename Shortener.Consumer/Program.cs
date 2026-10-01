using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Consumer;
using Shortener.Data;
using Shortener.Repository;
using Shortener.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<ICodeService, CodeService>();
builder.Services.AddScoped<ICodeRepository, CodeRepository>();
builder.Services.AddScoped<IKafkaConsumerService, KafkaConsumerService>();

var host = builder.Build();
host.Run();