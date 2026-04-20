using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class RefatorarComercialV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comercial_PedidosVendaItens_Engenharia_Produtos_ProdutoId",
                table: "Comercial_PedidosVendaItens");

            migrationBuilder.DropIndex(
                name: "IX_Comercial_PedidosVendaItens_ProdutoId",
                table: "Comercial_PedidosVendaItens");

            migrationBuilder.DropIndex(
                name: "IX_Comercial_NumerosSerie_PedidoVendaId",
                table: "Comercial_NumerosSerie");

            migrationBuilder.DropColumn(
                name: "PrecoUnitario",
                table: "Comercial_PedidosVendaItens");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Comercial_PedidosVendaItens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Comercial_NumerosSerie");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Comercial_NumerosSerie");

            migrationBuilder.RenameColumn(
                name: "Observacao",
                table: "Comercial_PedidoVendaHistorico",
                newName: "Justificativa");

            migrationBuilder.AddColumn<string>(
                name: "StatusAnterior",
                table: "Comercial_PedidoVendaHistorico",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusNovo",
                table: "Comercial_PedidoVendaHistorico",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Comercial_PedidosVendaItens",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observacao",
                table: "Comercial_PedidosVendaItens",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEntrega",
                table: "Comercial_PedidosVenda",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Comercial_PedidosVenda",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_NumerosSerie_PedidoVendaId",
                table: "Comercial_NumerosSerie",
                column: "PedidoVendaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comercial_NumerosSerie_PedidoVendaId",
                table: "Comercial_NumerosSerie");

            migrationBuilder.DropColumn(
                name: "StatusAnterior",
                table: "Comercial_PedidoVendaHistorico");

            migrationBuilder.DropColumn(
                name: "StatusNovo",
                table: "Comercial_PedidoVendaHistorico");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Comercial_PedidosVendaItens");

            migrationBuilder.DropColumn(
                name: "Observacao",
                table: "Comercial_PedidosVendaItens");

            migrationBuilder.DropColumn(
                name: "DataEntrega",
                table: "Comercial_PedidosVenda");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Comercial_PedidosVenda");

            migrationBuilder.RenameColumn(
                name: "Justificativa",
                table: "Comercial_PedidoVendaHistorico",
                newName: "Observacao");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoUnitario",
                table: "Comercial_PedidosVendaItens",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Comercial_PedidosVendaItens",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Comercial_NumerosSerie",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Comercial_NumerosSerie",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVendaItens_ProdutoId",
                table: "Comercial_PedidosVendaItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_NumerosSerie_PedidoVendaId",
                table: "Comercial_NumerosSerie",
                column: "PedidoVendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comercial_PedidosVendaItens_Engenharia_Produtos_ProdutoId",
                table: "Comercial_PedidosVendaItens",
                column: "ProdutoId",
                principalTable: "Engenharia_Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
