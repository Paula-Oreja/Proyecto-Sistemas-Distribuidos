
using FootballLeague.API.Data;
using FootballLeague.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LigasController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;

        public LigasController(FootballLeagueDbContext context)
        {
            _context = context;
        }

        // GET: api/ligas
        // Consultar todas las ligas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Liga>>> GetLigas()
        {
            var ligas = await _context.Ligas
                .AsNoTracking()
                .OrderBy(l => l.Nombre)
                .ThenBy(l => l.Temporada)
                .ToListAsync();

            return Ok(ligas);
        }

        // GET: api/ligas/1
        // Consultar una liga por su identificador
        [HttpGet("{id}")]
        public async Task<ActionResult<Liga>> GetLiga(int id)
        {
            var liga = await _context.Ligas
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (liga == null)
            {
                return NotFound("La liga no existe.");
            }

            return Ok(liga);
        }

        // POST: api/ligas
        // Crear una liga
        [HttpPost]
        public async Task<ActionResult<Liga>> CrearLiga(
            [FromBody] Liga nuevaLiga)
        {
            if (string.IsNullOrWhiteSpace(nuevaLiga.Nombre) ||
                string.IsNullOrWhiteSpace(nuevaLiga.Temporada))
            {
                return BadRequest(
                    "El nombre y la temporada son obligatorios.");
            }

            bool existe = await _context.Ligas.AnyAsync(
                l => l.Nombre == nuevaLiga.Nombre &&
                     l.Temporada == nuevaLiga.Temporada);

            if (existe)
            {
                return Conflict(
                    "Ya existe una liga con ese nombre y temporada.");
            }

            var liga = new Liga
            {
                Nombre = nuevaLiga.Nombre.Trim(),
                Temporada = nuevaLiga.Temporada.Trim(),
                Activa = nuevaLiga.Activa
            };

            _context.Ligas.Add(liga);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLiga),
                new { id = liga.Id },
                liga);
        }

        // PUT: api/ligas/1
        // Modificar una liga
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarLiga(
            int id,
            [FromBody] Liga datos)
        {
            var liga = await _context.Ligas.FindAsync(id);

            if (liga == null)
            {
                return NotFound("La liga no existe.");
            }

            if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Temporada))
            {
                return BadRequest(
                    "El nombre y la temporada son obligatorios.");
            }

            bool duplicada = await _context.Ligas.AnyAsync(
                l => l.Id != id &&
                     l.Nombre == datos.Nombre &&
                     l.Temporada == datos.Temporada);

            if (duplicada)
            {
                return Conflict(
                    "Ya existe otra liga con ese nombre y temporada.");
            }

            liga.Nombre = datos.Nombre.Trim();
            liga.Temporada = datos.Temporada.Trim();
            liga.Activa = datos.Activa;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ligas/1
        // Eliminar una liga sin jornadas ni inscripciones
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarLiga(int id)
        {
            var liga = await _context.Ligas.FindAsync(id);

            if (liga == null)
            {
                return NotFound("La liga no existe.");
            }

            bool tieneJornadas = await _context.Jornadas
                .AnyAsync(j => j.LigaId == id);

            bool tieneEquipos = await _context.ParticipacionesLiga
                .AnyAsync(p => p.LigaId == id);

            if (tieneJornadas || tieneEquipos)
            {
                return Conflict(
                    "No se puede eliminar una liga que tiene " +
                    "equipos inscritos o jornadas creadas.");
            }

            _context.Ligas.Remove(liga);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
