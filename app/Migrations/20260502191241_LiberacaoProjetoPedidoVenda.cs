using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class LiberacaoProjetoPedidoVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProdutoBomId",
                table: "Comercial_PedidosVenda",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVenda_ProdutoBomId",
                table: "Comercial_PedidosVenda",
                column: "ProdutoBomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comercial_PedidosVenda_Engenharia_Produtos_ProdutoBomId",
                table: "Comercial_PedidosVenda",
                column: "ProdutoBomId",
                principalTable: "Engenharia_Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comercial_PedidosVenda_Engenharia_Produtos_ProdutoBomId",
                table: "Comercial_PedidosVenda");

            migrationBuilder.DropIndex(
                name: "IX_Comercial_PedidosVenda_ProdutoBomId",
                table: "Comercial_PedidosVenda");

            migrationBuilder.DropColumn(
                name: "ProdutoBomId",
                table: "Comercial_PedidosVenda");
        }
    }
}
