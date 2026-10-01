using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data;
using Shortener.Repository;
using Shortener.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddScoped<ICodeService, CodeService>();
builder.Services.AddScoped<ICodeRepository, CodeRepository>();
builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
