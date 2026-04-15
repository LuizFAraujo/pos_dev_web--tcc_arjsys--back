using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoECodigoProjetoNoNS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoProjeto",
                table: "Comercial_NumerosSerie",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Comercial_NumerosSerie",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoProjeto",
                table: "Comercial_NumerosSerie");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Comercial_NumerosSerie");
        }
    }
}
