using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarComercial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comercial_PedidosVenda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercial_PedidosVenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comercial_PedidosVenda_Admin_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Admin_Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comercial_NumerosSerie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PedidoVendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercial_NumerosSerie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comercial_NumerosSerie_Comercial_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "Comercial_PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comercial_PedidosVendaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PedidoVendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercial_PedidosVendaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comercial_PedidosVendaItens_Comercial_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "Comercial_PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comercial_PedidosVendaItens_Engenharia_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Engenharia_Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_NumerosSerie_Codigo",
                table: "Comercial_NumerosSerie",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_NumerosSerie_PedidoVendaId",
                table: "Comercial_NumerosSerie",
                column: "PedidoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVenda_ClienteId",
                table: "Comercial_PedidosVenda",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVenda_Codigo",
                table: "Comercial_PedidosVenda",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVendaItens_PedidoVendaId",
                table: "Comercial_PedidosVendaItens",
                column: "PedidoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVendaItens_ProdutoId",
                table: "Comercial_PedidosVendaItens",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comercial_NumerosSerie");

            migrationBuilder.DropTable(
                name: "Comercial_PedidosVendaItens");

            migrationBuilder.DropTable(
                name: "Comercial_PedidosVenda");
        }
    }
}
