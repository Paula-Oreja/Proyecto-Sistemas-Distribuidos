
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

            // UN DORSAL NO PUEDE REPETIRSE
            // DENTRO DEL MISMO EQUIPO

            modelBuilder.Entity<Jugador>()
                .HasIndex(j => new { j.EquipoId, j.Dorsal })
                .IsUnique();

            // JORNADAS

            modelBuilder.Entity<Jornada>()
                .HasIndex(j => new { j.LigaId, j.Numero })
                .IsUnique();

            // PARTICIPACIONES

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

            modelBuilder.Entity<ParticipacionLiga>()
                .HasIndex(p => new { p.LigaId, p.EquipoId })
                .IsUnique();

            // CONTROL DE CONCURRENCIA

            modelBuilder.Entity<Partido>()
                .Property(p => p.Version)
                .IsConcurrencyToken();
        }
    }
}
