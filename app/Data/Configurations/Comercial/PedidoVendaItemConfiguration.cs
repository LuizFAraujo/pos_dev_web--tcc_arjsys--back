using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Comercial;

namespace Api_ArjSys_Tcc.Data.Configurations.Comercial;

public class PedidoVendaItemConfiguration : IEntityTypeConfiguration<PedidoVendaItem>
{
    public void Configure(EntityTypeBuilder<PedidoVendaItem> builder)
    {
        builder.ToTable("Comercial_PedidosVendaItens");

        builder.HasOne(i => i.PedidoVenda)
               .WithMany()
               .HasForeignKey(i => i.PedidoVendaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Produto)
               .WithMany()
               .HasForeignKey(i => i.ProdutoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.Quantidade).HasColumnType("decimal(18,4)");
        builder.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)");
    }
}