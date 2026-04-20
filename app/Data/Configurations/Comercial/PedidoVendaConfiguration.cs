using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Comercial;

namespace Api_ArjSys_Tcc.Data.Configurations.Comercial;

public class PedidoVendaConfiguration : IEntityTypeConfiguration<PedidoVenda>
{
    /// <summary>
    /// Configuração EF do Pedido de Venda.
    /// </summary>
    public void Configure(EntityTypeBuilder<PedidoVenda> builder)
    {
        builder.ToTable("Comercial_PedidosVenda");

        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.Property(p => p.Codigo).HasMaxLength(20);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Observacoes).HasMaxLength(500);

        builder.HasOne(p => p.Cliente)
               .WithMany()
               .HasForeignKey(p => p.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
