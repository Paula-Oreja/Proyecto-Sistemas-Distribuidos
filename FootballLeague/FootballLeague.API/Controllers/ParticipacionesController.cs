
using FootballLeague.API.Data;
using FootballLeague.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipacionesController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;

        public ParticipacionesController(
            FootballLeagueDbContext context)
        {
            _context = context;
        }

        // CONSULTAR LOS EQUIPOS INSCRITOS EN UNA LIGA
        // GET: api/Participaciones/liga/1

        [HttpGet("liga/{ligaId:int}")]
        public async Task<IActionResult> GetEquiposLiga(int ligaId)
        {
            bool existeLiga = await _context.Ligas
                .AnyAsync(l => l.Id == ligaId);

            if (!existeLiga)
            {
                return NotFound("La liga no existe.");
            }

            var equipos = await _context.ParticipacionesLiga
                .AsNoTracking()
                .Where(p => p.LigaId == ligaId)
                .OrderBy(p => p.Equipo!.Nombre)
                .Select(p => new
                {
                    p.Id,
                    p.LigaId,
                    p.EquipoId,
                    NombreEquipo = p.Equipo!.Nombre,
                    Ciudad = p.Equipo.Ciudad,
                    Estadio = p.Equipo.Estadio
                })
                .ToListAsync();

            return Ok(equipos);
        }

        // INSCRIBIR UN EQUIPO EN UNA LIGA
        // POST: api/Participaciones

        [HttpPost]
        public async Task<IActionResult> InscribirEquipo(
            [FromBody] SolicitudInscripcion solicitud)
        {
            bool existeLiga = await _context.Ligas
                .AnyAsync(l => l.Id == solicitud.LigaId);

            if (!existeLiga)
            {
                return NotFound("La liga no existe.");
            }

            bool existeEquipo = await _context.Equipos
                .AnyAsync(e => e.Id == solicitud.EquipoId);

            if (!existeEquipo)
            {
                return NotFound("El equipo no existe.");
            }

            bool yaInscrito = await _context.ParticipacionesLiga
                .AnyAsync(p =>
                    p.LigaId == solicitud.LigaId &&
                    p.EquipoId == solicitud.EquipoId);

            if (yaInscrito)
            {
                return Conflict(
                    "Este equipo ya está inscrito en la liga.");
            }

            var participacion = new ParticipacionLiga
            {
                LigaId = solicitud.LigaId,
                EquipoId = solicitud.EquipoId
            };

            _context.ParticipacionesLiga.Add(participacion);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo completar la inscripción. " +
                    "Comprueba si el equipo ya está inscrito.");
            }

            return StatusCode(201, new
            {
                Mensaje = "Equipo inscrito correctamente.",
                participacion.Id,
                participacion.LigaId,
                participacion.EquipoId
            });
        }

        // ELIMINAR UNA INSCRIPCIÓN
        // DELETE: api/Participaciones/1

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarInscripcion(int id)
        {
            var participacion = await _context.ParticipacionesLiga
                .FindAsync(id);

            if (participacion == null)
            {
                return NotFound("La inscripción no existe.");
            }

            bool tienePartidos = await _context.Partidos
                .AnyAsync(p =>
                    p.Jornada!.LigaId == participacion.LigaId &&
                    (p.EquipoLocalId == participacion.EquipoId ||
                     p.EquipoVisitanteId == participacion.EquipoId));

            if (tienePartidos)
            {
                return Conflict(
                    "No se puede retirar un equipo que " +
                    "ya tiene partidos programados.");
            }

            _context.ParticipacionesLiga.Remove(participacion);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class SolicitudInscripcion
    {
        public int LigaId { get; set; }

        public int EquipoId { get; set; }
    }
}
