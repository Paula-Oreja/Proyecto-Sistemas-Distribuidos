
namespace FootballLeague.API.Models
{
    public class Jugador
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public int Dorsal { get; set; }

        public string Posicion { get; set; } = "";

        public int EquipoId { get; set; }

        public Equipo? Equipo { get; set; }
    }
}
