
using FootballLeague.API.Data;
using FootballLeague.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClasificacionController : ControllerBase
    {
        private readonly FootballLeagueDbContext _context;
        private readonly ClasificacionService _service;

        public ClasificacionController(
            FootballLeagueDbContext context,
            ClasificacionService service)
        {
            _context = context;
            _service = service;
        }

        // GET: api/Clasificacion/liga/1

        [HttpGet("liga/{ligaId:int}")]
        public async Task<IActionResult> GetClasificacion(
            int ligaId)
        {
            bool existe = await _context.Ligas
                .AnyAsync(l => l.Id == ligaId);

            if (!existe)
            {
                return NotFound("La liga no existe.");
            }

            var clasificacion =
                await _service.CalcularClasificacion(ligaId);

            return Ok(clasificacion);
        }
    }
}
