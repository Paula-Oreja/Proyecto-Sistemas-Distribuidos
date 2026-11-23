
using FootballLeague.API.Data;
using FootballLeague.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquiposController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;

        public EquiposController(FootballLeagueDbContext context)
        {
            _context = context;
        }

        // GET: api/equipos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipo>>> GetEquipos()
        {
            var equipos = await _context.Equipos
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .Select(e => new
                {
                    e.Id,
                    e.Nombre,
                    e.Ciudad,
                    e.Estadio
                })
                .ToListAsync();

            return Ok(equipos);
        }

        // GET: api/equipos/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEquipo(int id)
        {
            var equipo = await _context.Equipos
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.Nombre,
                    e.Ciudad,
                    e.Estadio
                })
                .FirstOrDefaultAsync();

            if (equipo == null)
                return NotFound("El equipo no existe.");

            return Ok(equipo);
        }

        // POST: api/equipos
        [HttpPost]
        public async Task<IActionResult> CrearEquipo(
            [FromBody] Equipo datos)
        {
            if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Ciudad) ||
                string.IsNullOrWhiteSpace(datos.Estadio))
            {
                return BadRequest(
                    "Nombre, ciudad y estadio son obligatorios.");
            }

            string nombre = datos.Nombre.Trim();

            bool existe = await _context.Equipos
                .AnyAsync(e => e.Nombre == nombre);

            if (existe)
                return Conflict("Ya existe un equipo con ese nombre.");

            var equipo = new Equipo
            {
                Nombre = nombre,
                Ciudad = datos.Ciudad.Trim(),
                Estadio = datos.Estadio.Trim()
            };

            _context.Equipos.Add(equipo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEquipo),
                new { id = equipo.Id },
                new
                {
                    equipo.Id,
                    equipo.Nombre,
                    equipo.Ciudad,
                    equipo.Estadio
                });
        }

        // PUT: api/equipos/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ModificarEquipo(
            int id, [FromBody] Equipo datos)
        {
            var equipo = await _context.Equipos.FindAsync(id);

            if (equipo == null)
                return NotFound("El equipo no existe.");

            if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Ciudad) ||
                string.IsNullOrWhiteSpace(datos.Estadio))
            {
                return BadRequest(
                    "Nombre, ciudad y estadio son obligatorios.");
            }

            string nombre = datos.Nombre.Trim();

            bool duplicado = await _context.Equipos
                .AnyAsync(e => e.Id != id && e.Nombre == nombre);

            if (duplicado)
                return Conflict("Ya existe otro equipo con ese nombre.");

            equipo.Nombre = nombre;
            equipo.Ciudad = datos.Ciudad.Trim();
            equipo.Estadio = datos.Estadio.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/equipos/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarEquipo(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);

            if (equipo == null)
                return NotFound("El equipo no existe.");

            bool inscrito = await _context.ParticipacionesLiga
                .AnyAsync(p => p.EquipoId == id);

            bool tieneJugadores = await _context.Jugadores
                .AnyAsync(j => j.EquipoId == id);

            bool tienePartidos = await _context.Partidos
                .AnyAsync(p => p.EquipoLocalId == id ||
                               p.EquipoVisitanteId == id);

            if (inscrito || tieneJugadores || tienePartidos)
            {
                return Conflict(
                    "No se puede eliminar un equipo con " +
                    "inscripciones, jugadores o partidos asociados.");
            }

            _context.Equipos.Remove(equipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
