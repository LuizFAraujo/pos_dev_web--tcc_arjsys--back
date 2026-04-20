using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Producao;

namespace Api_ArjSys_Tcc.Data.Configurations.Producao;

public class OrdemProducaoItemConfiguration : IEntityTypeConfiguration<OrdemProducaoItem>
{
    /// <summary>
    /// Configuração EF dos itens da OP.
    /// QuantidadePlanejada é snapshot (imutável a não ser por edição manual).
    /// QuantidadeProduzida cresce via apontamentos.
    /// </summary>
    public void Configure(EntityTypeBuilder<OrdemProducaoItem> builder)
    {
        builder.ToTable("Producao_OrdensProducaoItens");

        builder.Property(i => i.Observacao).HasMaxLength(500);

        // Um produto aparece no máximo 1 vez por OP (soma na mesma linha)
        builder.HasIndex(i => new { i.OrdemProducaoId, i.ProdutoId }).IsUnique();

        builder.HasOne(i => i.OrdemProducao)
               .WithMany()
               .HasForeignKey(i => i.OrdemProducaoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Produto)
               .WithMany()
               .HasForeignKey(i => i.ProdutoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
