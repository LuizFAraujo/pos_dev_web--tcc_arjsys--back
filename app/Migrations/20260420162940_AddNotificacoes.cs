using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin_Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModuloDestino = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Lida = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataLeitura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OrigemTabela = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OrigemId = table.Column<int>(type: "INTEGER", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_Notificacoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_Notificacoes_ModuloDestino_Lida",
                table: "Admin_Notificacoes",
                columns: new[] { "ModuloDestino", "Lida" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_Notificacoes");
        }
    }
}
