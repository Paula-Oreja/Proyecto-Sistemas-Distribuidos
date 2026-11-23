
using FootballLeague.API.Data;
using FootballLeague.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultadosController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;

        public ResultadosController(
            FootballLeagueDbContext context)
        {
            _context = context;
        }

        // GET: api/Resultados/1
        // Consultar el estado actual de un partido

        [HttpGet("{partidoId:int}")]
        public async Task<IActionResult> ConsultarResultado(
            int partidoId)
        {
            var partido = await _context.Partidos
                .AsNoTracking()
                .Where(p => p.Id == partidoId)
                .Select(p => new
                {
                    p.Id,
                    p.EquipoLocalId,
                    EquipoLocal = p.EquipoLocal!.Nombre,
                    p.EquipoVisitanteId,
                    EquipoVisitante = p.EquipoVisitante!.Nombre,
                    p.GolesLocal,
                    p.GolesVisitante,
                    p.Finalizado,
                    p.Version
                })
                .FirstOrDefaultAsync();

            if (partido == null)
                return NotFound("El partido no existe.");

            return Ok(partido);
        }

        // PUT: api/Resultados/1
        // Registrar o modificar el marcador

        [HttpPut("{partidoId:int}")]
        public async Task<IActionResult> RegistrarResultado(
            int partidoId,
            [FromBody] ResultadoPartidoDTO datos)
        {
            if (datos.GolesLocal < 0 ||
                datos.GolesVisitante < 0)
            {
                return BadRequest(
                    "Los goles no pueden ser negativos.");
            }

            if (datos.Version < 1)
            {
                return BadRequest(
                    "Debes indicar una versión válida.");
            }

            // Actualización condicional directamente
            // en la base de datos para evitar que
            // dos usuarios sobrescriban sus resultados.

            int filasActualizadas = await _context.Partidos
                .Where(p =>
                    p.Id == partidoId &&
                    p.Version == datos.Version)
                .ExecuteUpdateAsync(actualizacion =>
                    actualizacion
                        .SetProperty(
                            p => p.GolesLocal,
                            (int?)datos.GolesLocal)
                        .SetProperty(
                            p => p.GolesVisitante,
                            (int?)datos.GolesVisitante)
                        .SetProperty(
                            p => p.Finalizado,
                            true)
                        .SetProperty(
                            p => p.Version,
                            p => p.Version + 1));

            if (filasActualizadas == 0)
            {
                bool existe = await _context.Partidos
                    .AnyAsync(p => p.Id == partidoId);

                if (!existe)
                    return NotFound("El partido no existe.");

                return Conflict(
                    "El partido ha sido modificado por " +
                    "otro usuario. Consulta la versión " +
                    "actual antes de guardar.");
            }

            var resultado = await _context.Partidos
                .AsNoTracking()
                .Where(p => p.Id == partidoId)
                .Select(p => new
                {
                    p.Id,
                    p.GolesLocal,
                    p.GolesVisitante,
                    p.Finalizado,
                    p.Version
                })
                .FirstAsync();

            return Ok(new
            {
                Mensaje = "Resultado guardado correctamente.",
                Partido = resultado
            });
        }
    }
}
