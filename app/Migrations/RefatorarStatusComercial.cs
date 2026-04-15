using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class RefatorarStatusComercial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Atualizar status dos PedidosVenda
            migrationBuilder.Sql("UPDATE Comercial_PedidosVenda SET Status = 'Aguardando' WHERE Status = 'Orcamento'");
            migrationBuilder.Sql("UPDATE Comercial_PedidosVenda SET Status = 'EmAndamento' WHERE Status = 'Aprovado'");
            migrationBuilder.Sql("UPDATE Comercial_PedidosVenda SET Status = 'EmAndamento' WHERE Status = 'EmProducao'");

            // Atualizar status dos NumerosSerie
            migrationBuilder.Sql("UPDATE Comercial_NumerosSerie SET Status = 'Aguardando' WHERE Status = 'Aberto'");
            migrationBuilder.Sql("UPDATE Comercial_NumerosSerie SET Status = 'EmAndamento' WHERE Status = 'EmFabricacao'");
            migrationBuilder.Sql("UPDATE Comercial_NumerosSerie SET Status = 'AguardandoEntrega' WHERE Status = 'Concluido'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}