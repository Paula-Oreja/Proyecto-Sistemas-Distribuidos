
using FootballLeague.API.Data;
using FootballLeague.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Services
{
    public class ClasificacionService
    {
        private readonly FootballLeagueDbContext _context;

        public ClasificacionService(
            FootballLeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClasificacionDTO>>
            CalcularClasificacion(int ligaId)
        {
            // Obtener los equipos inscritos

            var equipos = await _context.ParticipacionesLiga
                .AsNoTracking()
                .Where(p => p.LigaId == ligaId)
                .Select(p => new
                {
                    p.EquipoId,
                    Nombre = p.Equipo!.Nombre
                })
                .ToListAsync();

            // Crear una fila por cada equipo

            var tabla = equipos.ToDictionary(
                e => e.EquipoId,
                e => new ClasificacionDTO
                {
                    EquipoId = e.EquipoId,
                    Equipo = e.Nombre
                });

            // Obtener los partidos finalizados

            var partidos = await _context.Partidos
                .AsNoTracking()
                .Where(p =>
                    p.Jornada!.LigaId == ligaId &&
                    p.Finalizado &&
                    p.GolesLocal.HasValue &&
                    p.GolesVisitante.HasValue)
                .Select(p => new
                {
                    p.EquipoLocalId,
                    p.EquipoVisitanteId,
                    GolesLocal = p.GolesLocal!.Value,
                    GolesVisitante = p.GolesVisitante!.Value
                })
                .ToListAsync();

            foreach (var partido in partidos)
            {
                if (!tabla.TryGetValue(
                        partido.EquipoLocalId,
                        out var local) ||
                    !tabla.TryGetValue(
                        partido.EquipoVisitanteId,
                        out var visitante))
                {
                    continue;
                }

                local.PartidosJugados++;
                visitante.PartidosJugados++;

                local.GolesFavor += partido.GolesLocal;
                local.GolesContra += partido.GolesVisitante;

                visitante.GolesFavor += partido.GolesVisitante;
                visitante.GolesContra += partido.GolesLocal;

                if (partido.GolesLocal > partido.GolesVisitante)
                {
                    local.PartidosGanados++;
                    visitante.PartidosPerdidos++;
                }
                else if (partido.GolesLocal < partido.GolesVisitante)
                {
                    visitante.PartidosGanados++;
                    local.PartidosPerdidos++;
                }
                else
                {
                    local.PartidosEmpatados++;
                    visitante.PartidosEmpatados++;
                }
            }

            // Ordenar por puntos, diferencia de goles,
            // goles a favor y nombre.

            var clasificacion = tabla.Values
                .OrderByDescending(e => e.Puntos)
                .ThenByDescending(e => e.DiferenciaGoles)
                .ThenByDescending(e => e.GolesFavor)
                .ThenBy(e => e.Equipo)
                .ToList();

            for (int i = 0; i < clasificacion.Count; i++)
            {
                clasificacion[i].Posicion = i + 1;
            }

            return clasificacion;
        }
    }
}
