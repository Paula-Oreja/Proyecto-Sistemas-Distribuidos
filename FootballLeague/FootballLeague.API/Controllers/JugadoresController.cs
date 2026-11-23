
using FootballLeague.API.Data;
using FootballLeague.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JugadoresController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;

        public JugadoresController(
            FootballLeagueDbContext context)
        {
            _context = context;
        }

        // GET: api/Jugadores
        // Consultar todos los jugadores

        [HttpGet]
        public async Task<IActionResult> GetJugadores()
        {
            var jugadores = await _context.Jugadores
                .AsNoTracking()
                .OrderBy(j => j.Equipo!.Nombre)
                .ThenBy(j => j.Dorsal)
                .Select(j => new
                {
                    j.Id,
                    j.Nombre,
                    j.Dorsal,
                    j.Posicion,
                    j.EquipoId,
                    NombreEquipo = j.Equipo!.Nombre
                })
                .ToListAsync();

            return Ok(jugadores);
        }

        // GET: api/Jugadores/1
        // Consultar un jugador por ID

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJugador(int id)
        {
            var jugador = await _context.Jugadores
                .AsNoTracking()
                .Where(j => j.Id == id)
                .Select(j => new
                {
                    j.Id,
                    j.Nombre,
                    j.Dorsal,
                    j.Posicion,
                    j.EquipoId,
                    NombreEquipo = j.Equipo!.Nombre
                })
                .FirstOrDefaultAsync();

            if (jugador == null)
                return NotFound("El jugador no existe.");

            return Ok(jugador);
        }

        // GET: api/Jugadores/equipo/1
        // Consultar la plantilla de un equipo

        [HttpGet("equipo/{equipoId:int}")]
        public async Task<IActionResult> GetJugadoresEquipo(
            int equipoId)
        {
            bool existeEquipo = await _context.Equipos
                .AnyAsync(e => e.Id == equipoId);

            if (!existeEquipo)
                return NotFound("El equipo no existe.");

            var jugadores = await _context.Jugadores
                .AsNoTracking()
                .Where(j => j.EquipoId == equipoId)
                .OrderBy(j => j.Dorsal)
                .Select(j => new
                {
                    j.Id,
                    j.Nombre,
                    j.Dorsal,
                    j.Posicion,
                    j.EquipoId
                })
                .ToListAsync();

            return Ok(jugadores);
        }

        // POST: api/Jugadores
        // Registrar un jugador

        [HttpPost]
        public async Task<IActionResult> CrearJugador(
            [FromBody] DatosJugador datos)
        {
            if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Posicion))
            {
                return BadRequest(
                    "El nombre y la posición son obligatorios.");
            }

            if (datos.Dorsal < 1 || datos.Dorsal > 99)
            {
                return BadRequest(
                    "El dorsal debe estar entre 1 y 99.");
            }

            bool existeEquipo = await _context.Equipos
                .AnyAsync(e => e.Id == datos.EquipoId);

            if (!existeEquipo)
                return NotFound("El equipo no existe.");

            bool dorsalOcupado = await _context.Jugadores
                .AnyAsync(j =>
                    j.EquipoId == datos.EquipoId &&
                    j.Dorsal == datos.Dorsal);

            if (dorsalOcupado)
                return Conflict(
                    "Ese dorsal ya está ocupado en el equipo.");

            var jugador = new Jugador
            {
                Nombre = datos.Nombre.Trim(),
                Dorsal = datos.Dorsal,
                Posicion = datos.Posicion.Trim(),
                EquipoId = datos.EquipoId
            };

            _context.Jugadores.Add(jugador);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo registrar el jugador. " +
                    "Comprueba que el dorsal esté disponible.");
            }

            return CreatedAtAction(
                nameof(GetJugador),
                new { id = jugador.Id },
                new
                {
                    jugador.Id,
                    jugador.Nombre,
                    jugador.Dorsal,
                    jugador.Posicion,
                    jugador.EquipoId
                });
        }

        // PUT: api/Jugadores/1
        // Modificar un jugador

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ModificarJugador(
            int id, [FromBody] DatosJugador datos)
        {
            var jugador = await _context.Jugadores.FindAsync(id);

            if (jugador == null)
                return NotFound("El jugador no existe.");

            if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Posicion))
            {
                return BadRequest(
                    "El nombre y la posición son obligatorios.");
            }

            if (datos.Dorsal < 1 || datos.Dorsal > 99)
                return BadRequest("El dorsal debe estar entre 1 y 99.");

            bool existeEquipo = await _context.Equipos
                .AnyAsync(e => e.Id == datos.EquipoId);

            if (!existeEquipo)
                return NotFound("El equipo no existe.");

            bool dorsalOcupado = await _context.Jugadores
                .AnyAsync(j =>
                    j.Id != id &&
                    j.EquipoId == datos.EquipoId &&
                    j.Dorsal == datos.Dorsal);

            if (dorsalOcupado)
                return Conflict(
                    "Ese dorsal ya está ocupado en el equipo.");

            jugador.Nombre = datos.Nombre.Trim();
            jugador.Dorsal = datos.Dorsal;
            jugador.Posicion = datos.Posicion.Trim();
            jugador.EquipoId = datos.EquipoId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo modificar el jugador.");
            }

            return NoContent();
        }

        // DELETE: api/Jugadores/1
        // Eliminar un jugador

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarJugador(int id)
        {
            var jugador = await _context.Jugadores.FindAsync(id);

            if (jugador == null)
                return NotFound("El jugador no existe.");

            _context.Jugadores.Remove(jugador);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // Datos que se envían desde Swagger o Windows Forms

    public class DatosJugador
    {
        public string Nombre { get; set; } = "";

        public int Dorsal { get; set; }

        public string Posicion { get; set; } = "";

        public int EquipoId { get; set; }
    }
}

