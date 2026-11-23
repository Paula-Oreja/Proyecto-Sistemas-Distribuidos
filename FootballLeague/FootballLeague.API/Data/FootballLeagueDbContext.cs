
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

        public DbSet<ParticipacionLiga> ParticipacionesLiga
            => Set<ParticipacionLiga>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RELACIONES DE LOS PARTIDOS

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

            // RELACIÓN EQUIPOS - JUGADORES

            modelBuilder.Entity<Jugador>()
                .HasOne(j => j.Equipo)
                .WithMany(e => e.Jugadores)
                .HasForeignKey(j => j.EquipoId)
                .OnDelete(DeleteBehavior.Restrict);

            // UNA JORNADA NO PUEDE REPETIRSE EN UNA LIGA

            modelBuilder.Entity<Jornada>()
                .HasIndex(j => new { j.LigaId, j.Numero })
                .IsUnique();

            // RELACIÓN LIGAS - EQUIPOS

            modelBuilder.Entity<ParticipacionLiga>()
                .HasOne(p => p.Liga)
                .WithMany(l => l.Participaciones)
                .HasForeignKey(p => p.LigaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParticipacionLiga>()
                .HasOne(p => p.Equipo)
                .WithMany(e => e.Participaciones)
                .HasForeignKey(p => p.EquipoId)
                .OnDelete(DeleteBehavior.Restrict);

            // UN EQUIPO SOLO PUEDE INSCRIBIRSE
            // UNA VEZ EN LA MISMA LIGA

            modelBuilder.Entity<ParticipacionLiga>()
                .HasIndex(p => new { p.LigaId, p.EquipoId })
                .IsUnique();

            // CONTROL DE CONCURRENCIA DE RESULTADOS

            modelBuilder.Entity<Partido>()
                .Property(p => p.Version)
                .IsConcurrencyToken();
        }
    }
}

