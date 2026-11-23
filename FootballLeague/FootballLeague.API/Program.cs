
using FootballLeague.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrar los controladores de la API REST
builder.Services.AddControllers();

// Configurar SQLite
builder.Services.AddDbContext<FootballLeagueDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString(
                "FootballLeagueConnection")));

// Configurar OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Activar Swagger durante el desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
