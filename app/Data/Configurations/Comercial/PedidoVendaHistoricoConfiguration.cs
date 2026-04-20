using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Comercial;

namespace Api_ArjSys_Tcc.Data.Configurations.Comercial;

public class PedidoVendaHistoricoConfiguration : IEntityTypeConfiguration<PedidoVendaHistorico>
{
    /// <summary>
    /// Configuração EF do histórico de eventos do PV.
    /// </summary>
    public void Configure(EntityTypeBuilder<PedidoVendaHistorico> builder)
    {
        builder.ToTable("Comercial_PedidoVendaHistorico");

        builder.Property(h => h.Evento).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.StatusAnterior).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.StatusNovo).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.Justificativa).HasMaxLength(500);

        builder.HasOne(h => h.PedidoVenda)
               .WithMany()
               .HasForeignKey(h => h.PedidoVendaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
