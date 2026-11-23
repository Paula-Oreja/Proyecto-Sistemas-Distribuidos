
using System.Collections.Generic;

namespace FootballLeague.API.Models
{
    public class Liga
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Temporada { get; set; } = "";

        public bool Activa { get; set; } = true;

        public List<Jornada> Jornadas { get; set; } = new();
    }
}
