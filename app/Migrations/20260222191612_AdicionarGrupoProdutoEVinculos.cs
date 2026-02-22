using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarGrupoProdutoEVinculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GruposProdutos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nivel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    QtdCaracteres = table.Column<int>(type: "INTEGER", nullable: false),
                    PathDocumentos = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposProdutos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GruposVinculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GrupoPaiId = table.Column<int>(type: "INTEGER", nullable: false),
                    GrupoFilhoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposVinculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GruposVinculos_GruposProdutos_GrupoFilhoId",
                        column: x => x.GrupoFilhoId,
                        principalTable: "GruposProdutos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GruposVinculos_GruposProdutos_GrupoPaiId",
                        column: x => x.GrupoPaiId,
                        principalTable: "GruposProdutos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GruposProdutos_Codigo_Nivel",
                table: "GruposProdutos",
                columns: new[] { "Codigo", "Nivel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GruposVinculos_GrupoFilhoId",
                table: "GruposVinculos",
                column: "GrupoFilhoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposVinculos_GrupoPaiId_GrupoFilhoId",
                table: "GruposVinculos",
                columns: new[] { "GrupoPaiId", "GrupoFilhoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GruposVinculos");

            migrationBuilder.DropTable(
                name: "GruposProdutos");
        }
    }
}
