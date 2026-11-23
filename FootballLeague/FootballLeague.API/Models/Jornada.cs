
using System.Collections.Generic;

namespace FootballLeague.API.Models
{
    public class Jornada
    {
        public int Id { get; set; }

        public int Numero { get; set; }

        public int LigaId { get; set; }

        public Liga? Liga { get; set; }

        public List<Partido> Partidos { get; set; } = new();
    }
}
