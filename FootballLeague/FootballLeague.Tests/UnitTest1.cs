using FootballLeague.API.Data;
using FootballLeague.API.Models;
using FootballLeague.API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FootballLeague.Tests
{
    public class UnitTest1
    {
        private readonly CalendarioService calendarioService = new();

        private List<Jornada> GenerarCalendarioCompleto()
        {
            return calendarioService.GenerarCalendario(
                1,
                Enumerable.Range(1, 20).ToList(),
                new DateTime(2026, 12, 1));
        }

        // PRUEBA 1: Número de jornadas

        [Fact]
        public void Calendario_Genera38Jornadas()
        {
            var jornadas = GenerarCalendarioCompleto();

            Assert.Equal(38, jornadas.Count);
        }

        // PRUEBA 2: Número total de partidos

        [Fact]
        public void Calendario_Genera380Partidos()
        {
            var jornadas = GenerarCalendarioCompleto();

            int total = jornadas.Sum(j => j.Partidos.Count);

            Assert.Equal(380, total);
        }

        // PRUEBA 3: Partidos por jornada

        [Fact]
        public void Calendario_CadaJornadaTiene10Partidos()
        {
            var jornadas = GenerarCalendarioCompleto();

            Assert.All(jornadas, jornada =>
            {
                Assert.Equal(10, jornada.Partidos.Count);
            });
        }

        // PRUEBA 4: Partidos por equipo

        [Fact]
        public void Calendario_CadaEquipoJuega38Partidos()
        {
            var partidos = GenerarCalendarioCompleto()
                .SelectMany(j => j.Partidos)
                .ToList();

            for (int equipoId = 1; equipoId <= 20; equipoId++)
            {
                int cantidad = partidos.Count(p =>
                    p.EquipoLocalId == equipoId ||
                    p.EquipoVisitanteId == equipoId);

                Assert.Equal(38, cantidad);
            }
        }

        // PRUEBA 5: Ningún equipo juega contra sí mismo

        [Fact]
        public void Calendario_NoPermiteEnfrentamientosConsigoMismo()
        {
            var partidos = GenerarCalendarioCompleto()
                .SelectMany(j => j.Partidos);

            Assert.All(partidos, partido =>
            {
                Assert.NotEqual(
                    partido.EquipoLocalId,
                    partido.EquipoVisitanteId);
            });
        }

        // PRUEBA 6: Partidos de ida y vuelta

        [Fact]
        public void Calendario_TodosJueganIdaYVuelta()
        {
            var partidos = GenerarCalendarioCompleto()
                .SelectMany(j => j.Partidos)
                .ToList();

            for (int local = 1; local <= 20; local++)
            {
                for (int visitante = 1; visitante <= 20; visitante++)
                {
                    if (local == visitante)
                        continue;

                    int cantidad = partidos.Count(p =>
                        p.EquipoLocalId == local &&
                        p.EquipoVisitanteId == visitante);

                    Assert.Equal(1, cantidad);
                }
            }
        }

        // PRUEBA 7: Equipos repetidos

        [Fact]
        public void Calendario_RechazaEquiposRepetidos()
        {
            var equipos = new List<int> { 1, 2, 2, 4 };

            Assert.Throws<ArgumentException>(() =>
                calendarioService.GenerarCalendario(
                    1,
                    equipos,
                    DateTime.Today));
        }

        // PRUEBA 8: Número impar de equipos

        [Fact]
        public void Calendario_RechazaNumeroImparDeEquipos()
        {
            var equipos = new List<int> { 1, 2, 3 };

            Assert.Throws<ArgumentException>(() =>
                calendarioService.GenerarCalendario(
                    1,
                    equipos,
                    DateTime.Today));
        }

        // PRUEBA 9: Número insuficiente de equipos

        [Fact]
        public void Calendario_RechazaUnSoloEquipo()
        {
            var equipos = new List<int> { 1 };

            Assert.Throws<ArgumentException>(() =>
                calendarioService.GenerarCalendario(
                    1,
                    equipos,
                    DateTime.Today));
        }

        // PRUEBA 10: Estado inicial de los partidos

        [Fact]
        public void Calendario_PartidosComienzanPendientes()
        {
            var partidos = GenerarCalendarioCompleto()
                .SelectMany(j => j.Partidos);

            Assert.All(partidos, partido =>
            {
                Assert.False(partido.Finalizado);
                Assert.Equal(1, partido.Version);
                Assert.Null(partido.GolesLocal);
                Assert.Null(partido.GolesVisitante);
            });
        }

        // PRUEBA 11: Un equipo solo juega una vez por jornada

        [Fact]
        public void Calendario_EquiposNoSeRepitenEnUnaJornada()
        {
            var jornadas = GenerarCalendarioCompleto();

            foreach (var jornada in jornadas)
            {
                var equipos = jornada.Partidos
                    .SelectMany(p => new[]
                    {
                        p.EquipoLocalId,
                        p.EquipoVisitanteId
                    })
                    .ToList();

                Assert.Equal(20, equipos.Count);
                Assert.Equal(20, equipos.Distinct().Count());
            }
        }

        // BASE DE DATOS INDEPENDIENTE PARA LAS PRUEBAS

        private async Task PrepararDatos(
            FootballLeagueDbContext context,
            int golesLocal,
            int golesVisitante,
            bool finalizado = true)
        {
            await context.Database.EnsureCreatedAsync();

            var liga = new Liga
            {
                Id = 1,
                Nombre = "Liga de pruebas",
                Temporada = "2026/2027",
                Activa = true
            };

            var equipoLocal = new Equipo
            {
                Id = 1,
                Nombre = "Real Madrid",
                Ciudad = "Madrid",
                Estadio = "Santiago Bernabéu"
            };

            var equipoVisitante = new Equipo
            {
                Id = 2,
                Nombre = "Barcelona",
                Ciudad = "Barcelona",
                Estadio = "Spotify Camp Nou"
            };

            context.Ligas.Add(liga);

            context.Equipos.AddRange(
                equipoLocal,
                equipoVisitante);

            await context.SaveChangesAsync();

            context.ParticipacionesLiga.AddRange(
                new ParticipacionLiga
                {
                    LigaId = 1,
                    EquipoId = 1
                },
                new ParticipacionLiga
                {
                    LigaId = 1,
                    EquipoId = 2
                });

            var jornada = new Jornada
            {
                Id = 1,
                LigaId = 1,
                Numero = 1
            };

            context.Jornadas.Add(jornada);

            await context.SaveChangesAsync();

            context.Partidos.Add(new Partido
            {
                Id = 1,
                JornadaId = 1,
                EquipoLocalId = 1,
                EquipoVisitanteId = 2,
                Fecha = new DateTime(2026, 12, 1),
                GolesLocal = golesLocal,
                GolesVisitante = golesVisitante,
                Finalizado = finalizado,
                Version = 1
            });

            await context.SaveChangesAsync();
        }

        // PRUEBA 12: Una victoria suma tres puntos

        [Fact]
        public async Task Clasificacion_VictoriaSumaTresPuntos()
        {
            using var conexion =
                new SqliteConnection("Data Source=:memory:");

            await conexion.OpenAsync();

            var opciones =
                new DbContextOptionsBuilder<FootballLeagueDbContext>()
                    .UseSqlite(conexion)
                    .Options;

            using var context =
                new FootballLeagueDbContext(opciones);

            await PrepararDatos(context, 3, 1);

            var servicio = new ClasificacionService(context);

            var tabla = await servicio.CalcularClasificacion(1);

            var madrid = tabla.Single(e => e.EquipoId == 1);

            Assert.Equal(3, madrid.Puntos);
            Assert.Equal(1, madrid.PartidosGanados);
            Assert.Equal(1, madrid.PartidosJugados);
        }

        // PRUEBA 13: Una derrota suma cero puntos

        [Fact]
        public async Task Clasificacion_DerrotaSumaCeroPuntos()
        {
            using var conexion =
                new SqliteConnection("Data Source=:memory:");

            await conexion.OpenAsync();

            var opciones =
                new DbContextOptionsBuilder<FootballLeagueDbContext>()
                    .UseSqlite(conexion)
                    .Options;

            using var context =
                new FootballLeagueDbContext(opciones);

            await PrepararDatos(context, 3, 1);

            var servicio = new ClasificacionService(context);

            var tabla = await servicio.CalcularClasificacion(1);

            var barcelona = tabla.Single(e => e.EquipoId == 2);

            Assert.Equal(0, barcelona.Puntos);
            Assert.Equal(1, barcelona.PartidosPerdidos);
            Assert.Equal(1, barcelona.PartidosJugados);
        }

        // PRUEBA 14: Un empate suma un punto a cada equipo

        [Fact]
        public async Task Clasificacion_EmpateSumaUnPunto()
        {
            using var conexion =
                new SqliteConnection("Data Source=:memory:");

            await conexion.OpenAsync();

            var opciones =
                new DbContextOptionsBuilder<FootballLeagueDbContext>()
                    .UseSqlite(conexion)
                    .Options;

            using var context =
                new FootballLeagueDbContext(opciones);

            await PrepararDatos(context, 2, 2);

            var servicio = new ClasificacionService(context);

            var tabla = await servicio.CalcularClasificacion(1);

            Assert.Equal(2, tabla.Count);

            Assert.All(tabla, equipo =>
            {
                Assert.Equal(1, equipo.Puntos);
                Assert.Equal(1, equipo.PartidosEmpatados);
                Assert.Equal(1, equipo.PartidosJugados);
                Assert.Equal(0, equipo.DiferenciaGoles);
            });
        }

        // PRUEBA 15: Diferencia de goles

        [Fact]
        public async Task Clasificacion_CalculaDiferenciaDeGoles()
        {
            using var conexion =
                new SqliteConnection("Data Source=:memory:");

            await conexion.OpenAsync();

            var opciones =
                new DbContextOptionsBuilder<FootballLeagueDbContext>()
                    .UseSqlite(conexion)
                    .Options;

            using var context =
                new FootballLeagueDbContext(opciones);

            await PrepararDatos(context, 5, 1);

            var servicio = new ClasificacionService(context);

            var tabla = await servicio.CalcularClasificacion(1);

            var madrid = tabla.Single(e => e.EquipoId == 1);

            var barcelona = tabla.Single(e => e.EquipoId == 2);

            Assert.Equal(5, madrid.GolesFavor);
            Assert.Equal(1, madrid.GolesContra);
            Assert.Equal(4, madrid.DiferenciaGoles);

            Assert.Equal(1, barcelona.GolesFavor);
            Assert.Equal(5, barcelona.GolesContra);
            Assert.Equal(-4, barcelona.DiferenciaGoles);
        }

        // PRUEBA 16: Orden de la clasificación

        [Fact]
        public async Task Clasificacion_OrdenaPorPuntos()
        {
            using var conexion =
                new SqliteConnection("Data Source=:memory:");

            await conexion.OpenAsync();

            var opciones =
                new DbContextOptionsBuilder<FootballLeagueDbContext>()
                    .UseSqlite(conexion)
                    .Options;

            using var context =
                new FootballLeagueDbContext(opciones);

            await PrepararDatos(context, 3, 1);

            var servicio = new ClasificacionService(context);

            var tabla = await servicio.CalcularClasificacion(1);

            Assert.Equal(2, tabla.Count);

            Assert.Equal("Real Madrid", tabla[0].Equipo);
            Assert.Equal(1, tabla[0].Posicion);

            Assert.Equal("Barcelona", tabla[1].Equipo);
            Assert.Equal(2, tabla[1].Posicion);
        }

        // PRUEBA 17: Los partidos pendientes no cuentan

        [Fact]
        public async Task Clasificacion_IgnoraPartidosPendientes()
        {
            using var conexion =
                new SqliteConnection("Data Source=:memory:");

            await conexion.OpenAsync();

            var opciones =
                new DbContextOptionsBuilder<FootballLeagueDbContext>()
                    .UseSqlite(conexion)
                    .Options;

            using var context =
                new FootballLeagueDbContext(opciones);

            await PrepararDatos(context, 3, 1, false);

            var servicio = new ClasificacionService(context);

            var tabla = await servicio.CalcularClasificacion(1);

            Assert.Equal(2, tabla.Count);

            Assert.All(tabla, equipo =>
            {
                Assert.Equal(0, equipo.Puntos);
                Assert.Equal(0, equipo.PartidosJugados);
                Assert.Equal(0, equipo.GolesFavor);
                Assert.Equal(0, equipo.GolesContra);
            });
        }
    }
}