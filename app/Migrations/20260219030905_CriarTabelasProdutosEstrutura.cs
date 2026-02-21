using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelasProdutosEstrutura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produtos",
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
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstruturasProdutos",
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
                    table.PrimaryKey("PK_EstruturasProdutos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstruturasProdutos_Produtos_ProdutoFilhoId",
                        column: x => x.ProdutoFilhoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstruturasProdutos_Produtos_ProdutoPaiId",
                        column: x => x.ProdutoPaiId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstruturasProdutos_ProdutoFilhoId",
                table: "EstruturasProdutos",
                column: "ProdutoFilhoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstruturasProdutos_ProdutoPaiId_ProdutoFilhoId",
                table: "EstruturasProdutos",
                columns: new[] { "ProdutoPaiId", "ProdutoFilhoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_Codigo",
                table: "Produtos",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstruturasProdutos");

            migrationBuilder.DropTable(
                name: "Produtos");
        }
    }
}
