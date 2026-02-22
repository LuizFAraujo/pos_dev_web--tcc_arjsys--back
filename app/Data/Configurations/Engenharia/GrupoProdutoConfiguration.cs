using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data.Configurations.Engenharia;

public class GrupoProdutoConfiguration : IEntityTypeConfiguration<GrupoProduto>
{
    public void Configure(EntityTypeBuilder<GrupoProduto> builder)
    {
        builder.HasIndex(g => new { g.Codigo, g.Nivel }).IsUnique();
        builder.Property(g => g.Codigo).HasMaxLength(20);
        builder.Property(g => g.Descricao).HasMaxLength(100);
        builder.Property(g => g.Nivel).HasConversion<string>().HasMaxLength(20);
        builder.Property(g => g.PathDocumentos).HasMaxLength(500);
    }
}