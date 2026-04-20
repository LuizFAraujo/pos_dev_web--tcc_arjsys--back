using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Producao;

namespace Api_ArjSys_Tcc.Data.Configurations.Producao;

public class OrdemProducaoConfiguration : IEntityTypeConfiguration<OrdemProducao>
{
    /// <summary>
    /// Configuração EF da Ordem de Produção.
    /// Auto-relação para hierarquia Master → Filhas.
    /// </summary>
    public void Configure(EntityTypeBuilder<OrdemProducao> builder)
    {
        builder.ToTable("Producao_OrdensProducao");

        builder.HasIndex(o => o.Codigo).IsUnique();
        builder.Property(o => o.Codigo).HasMaxLength(30);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Observacoes).HasMaxLength(500);

        builder.HasOne(o => o.PedidoVenda)
               .WithMany()
               .HasForeignKey(o => o.PedidoVendaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Produto)
               .WithMany()
               .HasForeignKey(o => o.ProdutoId)
               .OnDelete(DeleteBehavior.Restrict);

        // Auto-relação Master/Filha — deletar Master apaga as filhas em cascata
        builder.HasOne(o => o.OrdemPai)
               .WithMany()
               .HasForeignKey(o => o.OrdemPaiId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
