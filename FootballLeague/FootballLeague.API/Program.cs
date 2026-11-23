
using FootballLeague.API.Data;
using FootballLeague.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controladores REST
builder.Services.AddControllers();

// Base de datos SQLite
builder.Services.AddDbContext<FootballLeagueDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString(
                "FootballLeagueConnection")));

// Servicios de lógica de negocio
builder.Services.AddScoped<CalendarioService>();
builder.Services.AddScoped<ClasificacionService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
