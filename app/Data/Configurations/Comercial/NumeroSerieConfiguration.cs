using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Comercial;

namespace Api_ArjSys_Tcc.Data.Configurations.Comercial;

public class NumeroSerieConfiguration : IEntityTypeConfiguration<NumeroSerie>
{
    /// <summary>
    /// Configuração EF do Número de Série.
    /// Relação 1:1 com PV garantida por índice único em PedidoVendaId.
    /// Produto é opcional (preenchido pela Engenharia conforme define o projeto).
    /// </summary>
    public void Configure(EntityTypeBuilder<NumeroSerie> builder)
    {
        builder.ToTable("Comercial_NumerosSerie");

        builder.HasIndex(n => n.Codigo).IsUnique();
        builder.HasIndex(n => n.PedidoVendaId).IsUnique();

        builder.Property(n => n.Codigo).HasMaxLength(20);

        builder.HasOne(n => n.PedidoVenda)
               .WithMany()
               .HasForeignKey(n => n.PedidoVendaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Produto)
               .WithMany()
               .HasForeignKey(n => n.ProdutoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
