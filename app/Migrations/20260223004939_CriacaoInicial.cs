using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Engenharia_GruposProdutos",
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
                    table.PrimaryKey("PK_Engenharia_GruposProdutos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    DescricaoCompleta = table.Column<string>(type: "TEXT", nullable: true),
                    Unidade = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Peso = table.Column<decimal>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_GruposVinculos",
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
                    table.PrimaryKey("PK_Engenharia_GruposVinculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engenharia_GruposVinculos_Engenharia_GruposProdutos_GrupoFilhoId",
                        column: x => x.GrupoFilhoId,
                        principalTable: "Engenharia_GruposProdutos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Engenharia_GruposVinculos_Engenharia_GruposProdutos_GrupoPaiId",
                        column: x => x.GrupoPaiId,
                        principalTable: "Engenharia_GruposProdutos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_EstruturasProdutos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProdutoPaiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoFilhoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    Posicao = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_EstruturasProdutos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engenharia_EstruturasProdutos_Engenharia_Produtos_ProdutoFilhoId",
                        column: x => x.ProdutoFilhoId,
                        principalTable: "Engenharia_Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Engenharia_EstruturasProdutos_Engenharia_Produtos_ProdutoPaiId",
                        column: x => x.ProdutoPaiId,
                        principalTable: "Engenharia_Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_EstruturasProdutos_ProdutoFilhoId",
                table: "Engenharia_EstruturasProdutos",
                column: "ProdutoFilhoId");

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_EstruturasProdutos_ProdutoPaiId_ProdutoFilhoId",
                table: "Engenharia_EstruturasProdutos",
                columns: new[] { "ProdutoPaiId", "ProdutoFilhoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_GruposProdutos_Codigo_Nivel",
                table: "Engenharia_GruposProdutos",
                columns: new[] { "Codigo", "Nivel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_GruposVinculos_GrupoFilhoId",
                table: "Engenharia_GruposVinculos",
                column: "GrupoFilhoId");

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_GruposVinculos_GrupoPaiId_GrupoFilhoId",
                table: "Engenharia_GruposVinculos",
                columns: new[] { "GrupoPaiId", "GrupoFilhoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_Produtos_Codigo",
                table: "Engenharia_Produtos",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Engenharia_EstruturasProdutos");

            migrationBuilder.DropTable(
                name: "Engenharia_GruposVinculos");

            migrationBuilder.DropTable(
                name: "Engenharia_Produtos");

            migrationBuilder.DropTable(
                name: "Engenharia_GruposProdutos");
        }
    }
}
