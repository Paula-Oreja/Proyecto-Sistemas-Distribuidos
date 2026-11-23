using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballLeague.API.Migrations
{
    /// <inheritdoc />
    public partial class DorsalesUnicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jugadores_EquipoId",
                table: "Jugadores");

            migrationBuilder.CreateIndex(
                name: "IX_Jugadores_EquipoId_Dorsal",
                table: "Jugadores",
                columns: new[] { "EquipoId", "Dorsal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jugadores_EquipoId_Dorsal",
                table: "Jugadores");

            migrationBuilder.CreateIndex(
                name: "IX_Jugadores_EquipoId",
                table: "Jugadores",
                column: "EquipoId");
        }
    }
}
