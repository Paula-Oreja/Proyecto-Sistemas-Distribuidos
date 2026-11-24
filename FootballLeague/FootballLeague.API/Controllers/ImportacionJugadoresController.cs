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
            int actualizados = 0;
            int sinCambios = 0;
            int omitidos = 0;

            var incidencias = new List<string>();

            var dorsalesProcesados =
                new HashSet<(int EquipoId, int Dorsal)>();

            foreach (var datos in jugadores)
            {
                if (datos == null ||
                    string.IsNullOrWhiteSpace(datos.Nombre) ||
                    string.IsNullOrWhiteSpace(datos.Posicion) ||
                    datos.Dorsal < 1 ||
                    datos.Dorsal > 99)
                {
                    omitidos++;

                    incidencias.Add(
                        $"Datos incorrectos: {datos?.Nombre}");

                    continue;
                }

                string nombre = datos.Nombre.Trim();
                string posicion = datos.Posicion.Trim();

                var equipo = equipos.FirstOrDefault(
                    e => e.Id == datos.EquipoId);

                if (equipo == null)
                {
                    omitidos++;

                    incidencias.Add(
                        $"Equipo inexistente: {datos.EquipoId}");

                    continue;
                }

                if (!dorsalesProcesados.Add(
                    (datos.EquipoId, datos.Dorsal)))
                {
                    omitidos++;

                    incidencias.Add(
                        $"Dorsal duplicado en el JSON: " +
                        $"{equipo.Nombre} - {datos.Dorsal}");

                    continue;
                }

                var jugadorExistente = existentes.FirstOrDefault(j =>
                    j.EquipoId == datos.EquipoId &&
                    j.Dorsal == datos.Dorsal);

                bool nombreDuplicado = existentes.Any(j =>
                    j.EquipoId == datos.EquipoId &&
                    j.Dorsal != datos.Dorsal &&
                    string.Equals(
                        j.Nombre.Trim(),
                        nombre,
                        StringComparison.OrdinalIgnoreCase));

                if (nombreDuplicado)
                {
                    omitidos++;

                    incidencias.Add(
                        $"Jugador con otro dorsal: " +
                        $"{nombre} - {equipo.Nombre}");

                    continue;
                }

                if (jugadorExistente != null)
                {
                    bool hayCambios =
                        jugadorExistente.Nombre != nombre ||
                        jugadorExistente.Posicion != posicion;

                    if (hayCambios)
                    {
                        jugadorExistente.Nombre = nombre;
                        jugadorExistente.Posicion = posicion;

                        actualizados++;
                    }
                    else
                    {
                        sinCambios++;
                    }

                    continue;
                }

                var jugador = new Jugador
                {
                    Nombre = nombre,

                    Dorsal = datos.Dorsal,

                    Posicion = posicion,

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
                Mensaje = "Importación completada correctamente.",

                Recibidos = jugadores.Count,

                Anadidos = anadidos,

                Actualizados = actualizados,

                SinCambios = sinCambios,

                Omitidos = omitidos,

                TotalJugadores = await _context.Jugadores.CountAsync(),

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