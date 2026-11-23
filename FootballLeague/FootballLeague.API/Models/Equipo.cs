
using System.Collections.Generic;

namespace FootballLeague.API.Models
{
    public class Equipo
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Ciudad { get; set; } = "";

        public string Estadio { get; set; } = "";

        public List<Jugador> Jugadores { get; set; } = new();

        public List<ParticipacionLiga> Participaciones { get; set; }
            = new();
    }
}


