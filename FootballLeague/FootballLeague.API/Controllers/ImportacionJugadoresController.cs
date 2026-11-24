
using FootballLeague.API.Data;
using FootballLeague.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportacionJugadoresController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;

        public ImportacionJugadoresController(
            FootballLeagueDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Importar(
            [FromBody] List<JugadorImportacion> jugadores)
        {
            if (jugadores == null || jugadores.Count == 0)
            {
                return BadRequest(
                    "No se han recibido jugadores.");
            }

            var equipos = await _context.Equipos
                .AsNoTracking()
                .ToListAsync();

            var existentes = await _context.Jugadores
                .ToListAsync();

            int anadidos = 0;
            int omitidos = 0;

            var incidencias = new List<string>();

            foreach (var datos in jugadores)
            {
                if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                    string.IsNullOrWhiteSpace(datos.Posicion) ||
                    datos.Dorsal < 1 ||
                    datos.Dorsal > 99)
                {
                    omitidos++;

                    incidencias.Add(
                        $"Datos incorrectos: {datos.Nombre}");

                    continue;
                }

                var equipo = equipos.FirstOrDefault(
                    e => e.Id == datos.EquipoId);

                if (equipo == null)
                {
                    omitidos++;

                    incidencias.Add(
                        $"Equipo inexistente: {datos.EquipoId}");

                    continue;
                }

                bool jugadorExiste = existentes.Any(j =>
                    j.EquipoId == datos.EquipoId &&
                    string.Equals(
                        j.Nombre.Trim(),
                        datos.Nombre.Trim(),
                        StringComparison.OrdinalIgnoreCase));

                if (jugadorExiste)
                {
                    omitidos++;

                    incidencias.Add(
                        $"Jugador existente: {datos.Nombre}");

                    continue;
                }

                bool dorsalOcupado = existentes.Any(j =>
                    j.EquipoId == datos.EquipoId &&
                    j.Dorsal == datos.Dorsal);

                if (dorsalOcupado)
                {
                    omitidos++;

                    incidencias.Add(
                        $"Dorsal ocupado: {datos.Nombre} - " +
                        $"{equipo.Nombre} - {datos.Dorsal}");

                    continue;
                }

                var jugador = new Jugador
                {
                    Nombre = datos.Nombre.Trim(),

                    Dorsal = datos.Dorsal,

                    Posicion = datos.Posicion.Trim(),

                    EquipoId = datos.EquipoId
                };

                _context.Jugadores.Add(jugador);

                existentes.Add(jugador);

                anadidos++;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo completar la importación. " +
                    "Comprueba los jugadores y dorsales existentes.");
            }

            return Ok(new
            {
                Mensaje = "Importación completada.",

                Recibidos = jugadores.Count,

                Anadidos = anadidos,

                Omitidos = omitidos,

                Incidencias = incidencias
            });
        }
    }

    public class JugadorImportacion
    {
        public string Nombre { get; set; } = "";

        public int Dorsal { get; set; }

        public string Posicion { get; set; } = "";

        public int EquipoId { get; set; }
    }
}
