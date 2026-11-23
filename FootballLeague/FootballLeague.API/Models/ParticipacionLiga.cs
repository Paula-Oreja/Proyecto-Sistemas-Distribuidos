
namespace FootballLeague.API.Models
{
    public class ParticipacionLiga
    {
        public int Id { get; set; }

        public int LigaId { get; set; }

        public Liga? Liga { get; set; }

        public int EquipoId { get; set; }

        public Equipo? Equipo { get; set; }
    }
}
