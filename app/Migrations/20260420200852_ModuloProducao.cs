using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class ModuloProducao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Producao_OrdensProducao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PedidoVendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrdemPaiId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producao_OrdensProducao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Producao_OrdensProducao_Comercial_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "Comercial_PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Producao_OrdensProducao_Engenharia_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Engenharia_Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Producao_OrdensProducao_Producao_OrdensProducao_OrdemPaiId",
                        column: x => x.OrdemPaiId,
                        principalTable: "Producao_OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Producao_OrdemProducaoHistorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemProducaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Evento = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StatusAnterior = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    StatusNovo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Justificativa = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Detalhe = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producao_OrdemProducaoHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Producao_OrdemProducaoHistorico_Producao_OrdensProducao_OrdemProducaoId",
                        column: x => x.OrdemProducaoId,
                        principalTable: "Producao_OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Producao_OrdensProducaoItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemProducaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadePlanejada = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantidadeProduzida = table.Column<decimal>(type: "TEXT", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producao_OrdensProducaoItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Producao_OrdensProducaoItens_Engenharia_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Engenharia_Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Producao_OrdensProducaoItens_Producao_OrdensProducao_OrdemProducaoId",
                        column: x => x.OrdemProducaoId,
                        principalTable: "Producao_OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdemProducaoHistorico_OrdemProducaoId",
                table: "Producao_OrdemProducaoHistorico",
                column: "OrdemProducaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdensProducao_Codigo",
                table: "Producao_OrdensProducao",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdensProducao_OrdemPaiId",
                table: "Producao_OrdensProducao",
                column: "OrdemPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdensProducao_PedidoVendaId",
                table: "Producao_OrdensProducao",
                column: "PedidoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdensProducao_ProdutoId",
                table: "Producao_OrdensProducao",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdensProducaoItens_OrdemProducaoId_ProdutoId",
                table: "Producao_OrdensProducaoItens",
                columns: new[] { "OrdemProducaoId", "ProdutoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdensProducaoItens_ProdutoId",
                table: "Producao_OrdensProducaoItens",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Producao_OrdemProducaoHistorico");

            migrationBuilder.DropTable(
                name: "Producao_OrdensProducaoItens");

            migrationBuilder.DropTable(
                name: "Producao_OrdensProducao");
        }
    }
}
