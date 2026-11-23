
using FootballLeague.API.Models;

namespace FootballLeague.API.Services
{
    public class CalendarioService
    {
        public List<Jornada> GenerarCalendario(
            int ligaId,
            List<int> equiposIds,
            DateTime fechaInicio)
        {
            if (equiposIds == null)
            {
                throw new ArgumentException(
                    "La lista de equipos no puede ser nula.");
            }

            if (equiposIds.Count < 2 ||
                equiposIds.Count % 2 != 0)
            {
                throw new ArgumentException(
                    "La liga necesita un número par " +
                    "de equipos, con un mínimo de dos.");
            }

            if (equiposIds.Distinct().Count()
                != equiposIds.Count)
            {
                throw new ArgumentException(
                    "No puede haber equipos repetidos.");
            }

            int cantidadEquipos = equiposIds.Count;

            int jornadasPorVuelta = cantidadEquipos - 1;

            int partidosPorJornada = cantidadEquipos / 2;

            // Copiamos los identificadores para rotarlos
            // sin modificar la lista original.

            var rotacion = new List<int>(equiposIds);

            var jornadas = new List<Jornada>();

            for (int ronda = 0;
                 ronda < jornadasPorVuelta;
                 ronda++)
            {
                var jornadaIda = new Jornada
                {
                    LigaId = ligaId,
                    Numero = ronda + 1
                };

                var jornadaVuelta = new Jornada
                {
                    LigaId = ligaId,
                    Numero = ronda + 1 + jornadasPorVuelta
                };

                for (int partido = 0;
                     partido < partidosPorJornada;
                     partido++)
                {
                    int equipoA = rotacion[partido];

                    int equipoB =
                        rotacion[cantidadEquipos - 1 - partido];

                    // Alternamos local y visitante
                    // para distribuir mejor los partidos.

                    bool invertir = (ronda + partido) % 2 == 1;

                    int local = invertir ? equipoB : equipoA;

                    int visitante = invertir ? equipoA : equipoB;

                    var fechaIda = fechaInicio.Date
                        .AddDays(ronda * 7)
                        .AddHours(18);

                    var fechaVuelta = fechaInicio.Date
                        .AddDays(
                            (ronda + jornadasPorVuelta) * 7)
                        .AddHours(18);

                    jornadaIda.Partidos.Add(new Partido
                    {
                        EquipoLocalId = local,
                        EquipoVisitanteId = visitante,
                        Fecha = fechaIda,
                        Finalizado = false,
                        Version = 1
                    });

                    jornadaVuelta.Partidos.Add(new Partido
                    {
                        EquipoLocalId = visitante,
                        EquipoVisitanteId = local,
                        Fecha = fechaVuelta,
                        Finalizado = false,
                        Version = 1
                    });
                }

                jornadas.Add(jornadaIda);
                jornadas.Add(jornadaVuelta);

                // Método del círculo:
                // dejamos fijo el primer equipo
                // y rotamos los demás.

                int ultimo =
                    rotacion[cantidadEquipos - 1];

                rotacion.RemoveAt(cantidadEquipos - 1);

                rotacion.Insert(1, ultimo);
            }

            return jornadas
                .OrderBy(j => j.Numero)
                .ToList();
        }
    }
}
