using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Producao;

namespace Api_ArjSys_Tcc.Data.Configurations.Producao;

public class OrdemProducaoHistoricoConfiguration : IEntityTypeConfiguration<OrdemProducaoHistorico>
{
    /// <summary>
    /// Configuração EF do histórico de eventos da OP.
    /// </summary>
    public void Configure(EntityTypeBuilder<OrdemProducaoHistorico> builder)
    {
        builder.ToTable("Producao_OrdemProducaoHistorico");

        builder.Property(h => h.Evento).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.StatusAnterior).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.StatusNovo).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.Justificativa).HasMaxLength(500);
        builder.Property(h => h.Detalhe).HasMaxLength(500);

        builder.HasOne(h => h.OrdemProducao)
               .WithMany()
               .HasForeignKey(h => h.OrdemProducaoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
