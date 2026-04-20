using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Comercial;

namespace Api_ArjSys_Tcc.Data.Configurations.Comercial;

public class PedidoVendaItemConfiguration : IEntityTypeConfiguration<PedidoVendaItem>
{
    /// <summary>
    /// Configuração EF do Item do Pedido de Venda (descrição livre).
    /// </summary>
    public void Configure(EntityTypeBuilder<PedidoVendaItem> builder)
    {
        builder.ToTable("Comercial_PedidosVendaItens");

        builder.HasOne(i => i.PedidoVenda)
               .WithMany()
               .HasForeignKey(i => i.PedidoVendaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Quantidade).HasColumnType("decimal(18,4)");
        builder.Property(i => i.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Observacao).HasMaxLength(200);
    }
}
