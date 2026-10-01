using Microsoft.EntityFrameworkCore;
using Shortener.Consumer;
using Shortener.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();