
using FootballLeague.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.API.Data
{
    public class FootballLeagueDbContext : DbContext
    {
        public FootballLeagueDbContext(
            DbContextOptions<FootballLeagueDbContext> options)
            : base(options)
        {
        }

        public DbSet<Liga> Ligas => Set<Liga>();

        public DbSet<Equipo> Equipos => Set<Equipo>();

        public DbSet<Jugador> Jugadores => Set<Jugador>();

        public DbSet<Jornada> Jornadas => Set<Jornada>();

        public DbSet<Partido> Partidos => Set<Partido>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoLocal)
                .WithMany()
                .HasForeignKey(p => p.EquipoLocalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoVisitante)
                .WithMany()
                .HasForeignKey(p => p.EquipoVisitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Jugador>()
                .HasOne(j => j.Equipo)
                .WithMany(e => e.Jugadores)
                .HasForeignKey(j => j.EquipoId);

            modelBuilder.Entity<Jornada>()
                .HasIndex(j => new { j.LigaId, j.Numero })
                .IsUnique();
        }
    }
}

