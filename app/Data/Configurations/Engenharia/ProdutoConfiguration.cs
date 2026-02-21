using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data.Configurations.Engenharia;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.Property(p => p.Codigo).HasMaxLength(50);
        builder.Property(p => p.Descricao).HasMaxLength(140);
        builder.Property(p => p.Unidade).HasConversion<string>().HasMaxLength(2);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
    }
}