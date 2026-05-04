using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class HistoricoVinculadoAoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrdemProducaoItemId",
                table: "Producao_OrdemProducaoHistorico",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producao_OrdemProducaoHistorico_OrdemProducaoItemId",
                table: "Producao_OrdemProducaoHistorico",
                column: "OrdemProducaoItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Producao_OrdemProducaoHistorico_Producao_OrdensProducaoItens_OrdemProducaoItemId",
                table: "Producao_OrdemProducaoHistorico",
                column: "OrdemProducaoItemId",
                principalTable: "Producao_OrdensProducaoItens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producao_OrdemProducaoHistorico_Producao_OrdensProducaoItens_OrdemProducaoItemId",
                table: "Producao_OrdemProducaoHistorico");

            migrationBuilder.DropIndex(
                name: "IX_Producao_OrdemProducaoHistorico_OrdemProducaoItemId",
                table: "Producao_OrdemProducaoHistorico");

            migrationBuilder.DropColumn(
                name: "OrdemProducaoItemId",
                table: "Producao_OrdemProducaoHistorico");
        }
    }
}
