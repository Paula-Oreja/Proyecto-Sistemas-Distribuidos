
using FootballLeague.API.Data;
using FootballLeague.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarioController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;
        private readonly CalendarioService _calendarioService;

        public CalendarioController(
            FootballLeagueDbContext context,
            CalendarioService calendarioService)
        {
            _context = context;
            _calendarioService = calendarioService;
        }

        // GET: api/Calendario/liga/1
        // Consultar todas las jornadas y partidos

        [HttpGet("liga/{ligaId:int}")]
        public async Task<IActionResult> ConsultarCalendario(
            int ligaId)
        {
            bool existe = await _context.Ligas
                .AnyAsync(l => l.Id == ligaId);

            if (!existe)
                return NotFound("La liga no existe.");

            var jornadas = await _context.Jornadas
                .AsNoTracking()
                .Where(j => j.LigaId == ligaId)
                .OrderBy(j => j.Numero)
                .Select(j => new
                {
                    j.Id,
                    j.Numero,
                    j.LigaId,
                    Partidos = j.Partidos
                        .OrderBy(p => p.Id)
                        .Select(p => new
                        {
                            p.Id,
                            p.Fecha,
                            p.EquipoLocalId,
                            EquipoLocal = p.EquipoLocal!.Nombre,
                            p.EquipoVisitanteId,
                            EquipoVisitante =
                                p.EquipoVisitante!.Nombre,
                            p.GolesLocal,
                            p.GolesVisitante,
                            p.Finalizado,
                            p.Version
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(jornadas);
        }

        // POST: api/Calendario/generar/1
        // Generar automáticamente todas las jornadas

        [HttpPost("generar/{ligaId:int}")]
        public async Task<IActionResult> GenerarCalendario(
            int ligaId,
            [FromBody] SolicitudCalendario solicitud)
        {
            var liga = await _context.Ligas
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == ligaId);

            if (liga == null)
                return NotFound("La liga no existe.");

            if (!liga.Activa)
                return BadRequest(
                    "La liga debe estar activa.");

            if (solicitud.FechaInicio == default)
                return BadRequest(
                    "Debes indicar una fecha de inicio válida.");

            // Obtener los equipos inscritos
            var equipos = await _context.ParticipacionesLiga
                .AsNoTracking()
                .Where(p => p.LigaId == ligaId)
                .OrderBy(p => p.EquipoId)
                .Select(p => p.EquipoId)
                .ToListAsync();

            if (equipos.Count < 2 || equipos.Count % 2 != 0)
            {
                return BadRequest(
                    "Para generar el calendario necesitas " +
                    "un número par de equipos (mínimo dos).");
            }

            try
            {
                // La transacción protege la creación
                // conjunta de jornadas y partidos.

                await using var transaccion =
                    await _context.Database
                        .BeginTransactionAsync();

                bool yaExiste = await _context.Jornadas
                    .AnyAsync(j => j.LigaId == ligaId);

                if (yaExiste)
                {
                    return Conflict(
                        "Esta liga ya tiene un calendario. " +
                        "No puedes generarlo dos veces.");
                }

                var jornadas =
                    _calendarioService.GenerarCalendario(
                        ligaId,
                        equipos,
                        solicitud.FechaInicio);

                _context.Jornadas.AddRange(jornadas);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                return StatusCode(201, new
                {
                    Mensaje = "Calendario generado correctamente.",
                    LigaId = ligaId,
                    TotalEquipos = equipos.Count,
                    TotalJornadas = jornadas.Count,
                    TotalPartidos = jornadas.Sum(
                        j => j.Partidos.Count)
                });
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqliteException sql &&
                      sql.SqliteErrorCode == 19)
            {
                return Conflict(
                    "No se pudo generar el calendario. " +
                    "Comprueba si ya existen jornadas.");
            }
            catch (SqliteException ex)
                when (ex.SqliteErrorCode == 5 ||
                      ex.SqliteErrorCode == 6)
            {
                return StatusCode(503,
                    "La base de datos está ocupada. " +
                    "Inténtalo nuevamente.");
            }
        }
    }

    public class SolicitudCalendario
    {
        public DateTime FechaInicio { get; set; }
    }
}

