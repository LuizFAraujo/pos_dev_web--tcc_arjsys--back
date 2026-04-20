using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class NsComProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoProjeto",
                table: "Comercial_NumerosSerie");

            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Comercial_NumerosSerie",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_NumerosSerie_ProdutoId",
                table: "Comercial_NumerosSerie",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comercial_NumerosSerie_Engenharia_Produtos_ProdutoId",
                table: "Comercial_NumerosSerie",
                column: "ProdutoId",
                principalTable: "Engenharia_Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comercial_NumerosSerie_Engenharia_Produtos_ProdutoId",
                table: "Comercial_NumerosSerie");

            migrationBuilder.DropIndex(
                name: "IX_Comercial_NumerosSerie_ProdutoId",
                table: "Comercial_NumerosSerie");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Comercial_NumerosSerie");

            migrationBuilder.AddColumn<string>(
                name: "CodigoProjeto",
                table: "Comercial_NumerosSerie",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }
    }
}
