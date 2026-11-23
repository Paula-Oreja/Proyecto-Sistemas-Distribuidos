
namespace FootballLeague.API.DTOs
{
    public class ClasificacionDTO
    {
        public int Posicion { get; set; }

        public int EquipoId { get; set; }

        public string Equipo { get; set; } = "";

        public int PartidosJugados { get; set; }

        public int PartidosGanados { get; set; }

        public int PartidosEmpatados { get; set; }

        public int PartidosPerdidos { get; set; }

        public int GolesFavor { get; set; }

        public int GolesContra { get; set; }

        public int DiferenciaGoles =>
            GolesFavor - GolesContra;

        public int Puntos =>
            PartidosGanados * 3 + PartidosEmpatados;
    }
}
