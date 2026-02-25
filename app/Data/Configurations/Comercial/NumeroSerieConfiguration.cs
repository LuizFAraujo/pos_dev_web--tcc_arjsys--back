using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Comercial;

namespace Api_ArjSys_Tcc.Data.Configurations.Comercial;

public class NumeroSerieConfiguration : IEntityTypeConfiguration<NumeroSerie>
{
    public void Configure(EntityTypeBuilder<NumeroSerie> builder)
    {
        builder.ToTable("Comercial_NumerosSerie");

        builder.HasIndex(n => n.Codigo).IsUnique();
        builder.Property(n => n.Codigo).HasMaxLength(20);
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(n => n.PedidoVenda)
               .WithMany()
               .HasForeignKey(n => n.PedidoVendaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}