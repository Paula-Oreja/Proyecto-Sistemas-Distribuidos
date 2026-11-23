
using System;

namespace FootballLeague.API.Models
{
    public class Partido
    {
        public int Id { get; set; }

        public int JornadaId { get; set; }

        public Jornada? Jornada { get; set; }

        public int EquipoLocalId { get; set; }

        public Equipo? EquipoLocal { get; set; }

        public int EquipoVisitanteId { get; set; }

        public Equipo? EquipoVisitante { get; set; }

        public DateTime Fecha { get; set; }

        public int? GolesLocal { get; set; }

        public int? GolesVisitante { get; set; }

        public bool Finalizado { get; set; } = false;

        public int Version { get; set; } = 1;
    }
}


